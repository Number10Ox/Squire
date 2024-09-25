using Unity.Collections;
using Unity.Entities;

public struct ActiveAgentActionTypes : IComponentData
{
    public BitField32 ActiveActionsMask;
}