using Unity.Entities;

public enum AgentActionState { NotStarted, Running, Done }
public enum AgentActionResult { Pending, Success, Fail }

public struct AgentAction : IComponentData
{
    public AgentActionType Type;
    public AgentActionState State;
    public AgentActionResult Result;
    public int Priority;
    public bool Interrupt;
}