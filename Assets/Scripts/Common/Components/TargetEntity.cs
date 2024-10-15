
using Unity.Entities;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct TargetEntity : IInputComponentData
{
    public Entity Target;
    [GhostField] public bool IsSet;
}