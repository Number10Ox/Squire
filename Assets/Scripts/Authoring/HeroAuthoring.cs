using Trove.UtilityAI;
using Unity.Entities;
using UnityEngine;

public class HeroAuthoring : MonoBehaviour
{
    public HeroConsiderationSetData ConsiderationSetData;

    public float DecisionInertia = 0.5f;
    public float MaxDistanceFromSquire = 100.0f;

    public class HeroBaker : Baker<HeroAuthoring>
    {
        public override void Bake(HeroAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HeroTag());
            AddComponent(entity, new AgentTag());
            AddBuffer<AgentPendingActionData>(entity);
            AddBuffer<AgentActiveActionData>(entity);
            AddComponent(entity, new AgentActiveActionType());

            HeroAI heroAI = new HeroAI();
            heroAI.DecisionInertia = authoring.DecisionInertia;
            heroAI.ShouldUpdateReasoner = true;
            heroAI.TimeSinceMadeDecision = float.MaxValue;
            heroAI.MaxDistanceFromSquire = authoring.MaxDistanceFromSquire;

            authoring.ConsiderationSetData.Bake(this, out HeroConsiderationSet considerationSetComponent);
            ReasonerUtilities.BeginBakeReasoner(this, out Reasoner reasoner, out DynamicBuffer<Action> actionsBuffer,
                out DynamicBuffer<Consideration> considerationsBuffer,
                out DynamicBuffer<ConsiderationInput> considerationInputsBuffer);
            {
                ReasonerUtilities.AddAction(new ActionDefinition((int)HeroAIAction.Idle), true, ref reasoner,
                    actionsBuffer, out heroAI.IdleRef);
                ReasonerUtilities.AddAction(new ActionDefinition((int)HeroAIAction.ReturnToSquire), true, ref reasoner,
                    actionsBuffer, out heroAI.ReturnToSquireRef);

                ReasonerUtilities.AddConsideration(considerationSetComponent.IdlingComfort, ref heroAI.IdleRef, true,
                    ref reasoner, actionsBuffer, considerationsBuffer, considerationInputsBuffer,
                    out heroAI.IdlingComfortRef);
                ReasonerUtilities.AddConsideration(considerationSetComponent.DistanceFromSquire,
                    ref heroAI.ReturnToSquireRef, true, ref reasoner, actionsBuffer, considerationsBuffer,
                    considerationInputsBuffer, out heroAI.DistanceFromSquireRef);
            }
            ReasonerUtilities.EndBakeReasoner(this, reasoner);

            AddComponent(entity, heroAI);
            DependsOn(authoring.ConsiderationSetData);
        }
    }
}