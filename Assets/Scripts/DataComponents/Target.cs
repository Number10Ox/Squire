
using Unity.Entities;
using Unity.Mathematics;

public struct Target : IComponentData
{
    public float3 targetPosition;
    public Entity targetEntity;
}