using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct TargetPosition  : IInputComponentData
{
    [GhostField(Quantization = 0)] public float3 Position; 
    [GhostField] public bool IsSet;
}
