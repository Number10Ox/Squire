
using Unity.Entities;

[InternalBufferCapacity(4)]
public struct ActiveAgentAction : IBufferElementData
{
    public Entity ActionEntity;
}