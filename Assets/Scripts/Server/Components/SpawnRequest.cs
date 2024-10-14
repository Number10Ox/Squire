using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct SpawnRequestElement : IBufferElementData
{
    public Entity Prefab;
    public float3 InitialPosition;
    public float Radius;
    public Entity SourceConnection;
    public FixedString32Bytes PlayerId;
}