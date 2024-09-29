using Unity.Entities;

public struct AgentPendingAction : IBufferElementData
{
    public Entity ActionEntity;
}