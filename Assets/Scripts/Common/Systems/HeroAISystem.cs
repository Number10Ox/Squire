using Trove.UtilityAI;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct HeroAISystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HeroTag>();
    }

    [BurstCompile]
    void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // TODO You need to find squire for the hero... and not expect a singleton squire
        var squireEntity = SystemAPI.GetSingletonEntity<SquireTag>();
        var agentTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        HeroAIUpdateJob updateJob = new HeroAIUpdateJob
        {
            Ecb = ecb.AsParallelWriter(),
            SquireEntity = squireEntity,
            AgentTransformLookup = agentTransformLookup,
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        state.Dependency = updateJob.Schedule(state.Dependency);
    }

    [BurstCompile]
    public partial struct HeroAIUpdateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public Entity SquireEntity;

        [ReadOnly]
        public ComponentLookup<LocalTransform> AgentTransformLookup;

        public float DeltaTime;

        void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref HeroAI heroAI,
            ref Reasoner reasoner, ref DynamicBuffer<Action> actionsBuffer,
            ref DynamicBuffer<Consideration> considerationsBuffer,
            ref DynamicBuffer<ConsiderationInput> considerationInputsBuffer)
        {
            heroAI.TimeSinceMadeDecision += DeltaTime;
            if (heroAI.ShouldUpdateReasoner && heroAI.TimeSinceMadeDecision > heroAI.DecisionInertia)
            {
                DecideOnAction(entity, sortKey, ref heroAI, ref reasoner, ref actionsBuffer, ref considerationsBuffer,
                    ref considerationInputsBuffer);
            }
        }

        private void DecideOnAction(Entity entity, [EntityIndexInQuery] int sortKey, ref HeroAI heroAI,
            ref Reasoner reasoner, ref DynamicBuffer<Action> actionsBuffer,
            ref DynamicBuffer<Consideration> considerationsBuffer,
            ref DynamicBuffer<ConsiderationInput> considerationInputsBuffer)
        {
            var squireTransform = AgentTransformLookup[SquireEntity];
            var heroTransform = AgentTransformLookup[entity];
            float distanceFromSquire = math.distance(heroTransform.Position, squireTransform.Position);
            float normalizedDistanceFromSquire = math.saturate(distanceFromSquire / heroAI.MaxDistanceFromSquire);

            ReasonerUtilities.SetConsiderationInput(ref heroAI.DistanceFromSquireRef, normalizedDistanceFromSquire,
                in reasoner, considerationsBuffer, considerationInputsBuffer);
            ReasonerUtilities.SetConsiderationInput(ref heroAI.IdlingComfortRef, normalizedDistanceFromSquire,
                in reasoner, considerationsBuffer, considerationInputsBuffer);

            ActionSelectors.HighestScoring actionSelector = new ActionSelectors.HighestScoring();

            if (ReasonerUtilities.UpdateScoresAndSelectAction(ref actionSelector, ref reasoner, actionsBuffer,
                    considerationsBuffer, considerationInputsBuffer, out Action selectedAction))
            {
                if (selectedAction.Score > 0f) // Don't bother switching actions if the new one scored 0
                {
                    ExecuteSelectedAction((HeroAIAction)selectedAction.Type, entity, sortKey, ref heroAI);
                    heroAI.TimeSinceMadeDecision = 0f;
                }
            }
        }

        private void ExecuteSelectedAction(HeroAIAction action, Entity entity, [EntityIndexInQuery] int sortKey,
            ref HeroAI heroAI)
        {
            if (action == HeroAIAction.ReturnToSquire)
            {
                var squireTransform = AgentTransformLookup[SquireEntity]; 
 
                var actionEntity = Ecb.CreateEntity(sortKey);
                Ecb.AddComponent(sortKey, actionEntity, new AgentMoveToPositionAction
                {
                    TargetPosition = squireTransform.Position
                });
                Ecb.AddComponent(sortKey, actionEntity, new AgentAction()
                {
                    Type = AgentActionType.MoveTo,
                    Priority = 1,
                    Interrupt = true,
                    State = AgentActionState.NotStarted,
                    Result = AgentActionResult.Pending
                }); 
                Ecb.AppendToBuffer(sortKey, entity, new AgentPendingActionElement()
                {
                    ActionEntity = actionEntity
                });
            }

            if (action == HeroAIAction.Idle)
            {
                // Debug.Log("Hero Action: Idle");
            }
        }
    }
}