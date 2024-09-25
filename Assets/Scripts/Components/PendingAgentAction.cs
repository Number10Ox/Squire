using Unity.Entities;

public struct PendingAgentAction : IBufferElementData
{
    public Entity ActionEntity;
}