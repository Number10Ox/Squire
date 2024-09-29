
using Unity.Entities;

[InternalBufferCapacity(4)]
public struct AgentActiveAction : IBufferElementData
{
    public Entity ActionEntity;
}