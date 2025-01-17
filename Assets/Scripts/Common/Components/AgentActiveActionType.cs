using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[System.Flags]
public enum AgentActionType : uint
{
    None = 0,
    MoveTo = 1 << 0,
    Sequence = 1 << 1,
    Interact = 1 << 2,
    Idle = 1 << 3,
    // etc.
}

public struct AgentActiveActionType : IComponentData
{
    public BitField32 ActiveActionsMask;

    public bool Has(AgentActionType actionType)
    {
        return (ActiveActionsMask.Value & (uint)actionType) != 0;
    }

    public void Add(AgentActionType actionType)
    {
        ActiveActionsMask.Value |= (uint)actionType;
    }

    public void Remove(AgentActionType actionType)
    {
        ActiveActionsMask.Value &= ~(uint)actionType;
    }

    public void Clear()
    {
        ActiveActionsMask.Value = 0;
    }
}