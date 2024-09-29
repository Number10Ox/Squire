
using Unity.Entities;

[InternalBufferCapacity(4)]
public struct AgentActiveActionData : IBufferElementData
{
    public Entity ActionEntity;
}