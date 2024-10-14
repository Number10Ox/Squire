
using Unity.Entities;

[InternalBufferCapacity(4)]
public struct AgentActiveActionElement : IBufferElementData
{
    public Entity ActionEntity;
}