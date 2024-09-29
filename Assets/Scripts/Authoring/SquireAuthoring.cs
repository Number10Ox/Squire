using Unity.Entities;
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
            AddBuffer<AgentPendingAction>(entity);
            AddBuffer<AgentActiveAction>(entity);
            AddComponent(entity, new AgentActiveActionTypes());
        }
    }
}

// Unmanaged type for SquireChildrenCount
public struct SquireChildrenCount : IComponentData
{
    public int Value;
}

