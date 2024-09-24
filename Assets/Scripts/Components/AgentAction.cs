using System.Collections.Generic;
using Unity.Entities;

public struct ActionPriorityComparer : IComparer<AgentAction>
{
    public int Compare(AgentAction action1, AgentAction action2)
    {
        if (action1.Priority < action2.Priority) return -1;
        if (action1.Priority > action2.Priority) return -1;
        return 0;
    }
}

[InternalBufferCapacity(8)]
public struct AgentAction : IBufferElementData
{
    public int Priority;
    public bool CanInterrupt;
    public bool CanRunInParallel;
}