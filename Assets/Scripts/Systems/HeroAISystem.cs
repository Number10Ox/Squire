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

        var squireEntity = SystemAPI.GetSingletonEntity<SquireTag>();
        var agentTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        HeroAIUpdateJob updateJob = new HeroAIUpdateJob
        {
            Ecb = ecb,
            SquireEntity = squireEntity,
            AgentTransformLookup = agentTransformLookup,
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        state.Dependency = updateJob.Schedule(state.Dependency);
    }

    [BurstCompile]
    public partial struct HeroAIUpdateJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public Entity SquireEntity;
        [ReadOnly]
        public ComponentLookup<LocalTransform> AgentTransformLookup;
        public float DeltaTime;

        void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref HeroAI heroAI, ref Reasoner reasoner,
            ref DynamicBuffer<Action> actionsBuffer, ref DynamicBuffer<Consideration> considerationsBuffer,
            ref DynamicBuffer<ConsiderationInput> considerationInputsBuffer)
        {
            heroAI.TimeSinceMadeDecision += DeltaTime;

            if (heroAI.ShouldUpdateReasoner && heroAI.TimeSinceMadeDecision > heroAI.DecisionInertia)
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
                        if ((HeroAIAction)selectedAction.Type == HeroAIAction.ReturnToSquire)
                        {
                            Debug.Log("Hero Action: Return To Squire");
                        }

                        if ((HeroAIAction)selectedAction.Type == HeroAIAction.Idle)
                        {
                            Debug.Log("Hero Action: Idle");
                        }

                        heroAI.TimeSinceMadeDecision = 0f;
                    }
                }
            }
        }
    }
}