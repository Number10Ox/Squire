using Unity.Entities;

public struct AgentPendingActionElement : IBufferElementData
{
    public Entity ActionEntity;
}