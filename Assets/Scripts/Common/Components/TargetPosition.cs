using Unity.Entities;
using Unity.Mathematics;

public struct TargetPosition  : IComponentData, IEnableableComponent
{
    public float3 Position; 
}
