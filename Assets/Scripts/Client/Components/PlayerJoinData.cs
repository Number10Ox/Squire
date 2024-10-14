using Unity.Collections;
using Unity.Entities;

public struct PlayerJoinData : IComponentData
{
    public FixedString32Bytes PlayerId;
}