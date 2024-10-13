using Unity.Entities;
using Unity.Mathematics;

public struct AgentMoveToPositionAction : IComponentData
{
    public float3 TargetPosition; 
}