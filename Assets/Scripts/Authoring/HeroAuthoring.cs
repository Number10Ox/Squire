using Unity.Entities;
using UnityEngine;

public class HeroAuthoring : MonoBehaviour
{
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
        }
    }
}