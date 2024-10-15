using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SquireAuthoring : MonoBehaviour
{
    public class SquireBaker : Baker<SquireAuthoring>
    {
        public override void Bake(SquireAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SquireTag());
            AddComponent(entity, new AgentTag());
            AddBuffer<AgentPendingActionElement>(entity);
            AddBuffer<AgentActiveActionElement>(entity);
            AddComponent(entity, new AgentActiveActionType());
            
            AddComponent(entity, new TargetPosition()
            {
                Position = float3.zero,
                IsSet = false
            });
            AddComponent(entity, new TargetEntity()
            {
                Target = Entity.Null,
                IsSet = false
            });
        }
    }
}

// Unmanaged type for SquireChildrenCount
public struct SquireChildrenCount : IComponentData
{
    public int Value;
}

