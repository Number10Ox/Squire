using Unity.Entities;

public enum AgentActionState { NotStarted, Running, Done }

public struct AgentAction : IComponentData
{
    public AgentActionType Type;
    public AgentActionState State;
    public int Priority;
    public bool Interrupt;
}