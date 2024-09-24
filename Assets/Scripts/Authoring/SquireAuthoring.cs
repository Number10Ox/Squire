using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SquireAuthoring : MonoBehaviour
{
    public class SquireBaker : Baker<SquireAuthoring>
    {
        public override void Bake(SquireAuthoring authoring)
        {
            var parent = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(parent, new SquireTag());
            AddComponent(parent, new AgentTag());
            AddBuffer<AgentAction>();
        }
    }
}

// Unmanaged type for SquireChildrenCount
public struct SquireChildrenCount : IComponentData
{
    public int Value;
}

