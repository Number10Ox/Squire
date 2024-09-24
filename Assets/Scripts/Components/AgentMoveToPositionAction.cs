using Unity.Entities;
using Unity.Mathematics;

public struct MoveToPositionAction : IComponentData
{
    public float3 targetPosition; 
}