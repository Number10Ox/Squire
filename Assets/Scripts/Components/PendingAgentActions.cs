
using ProjectDawn.Collections.LowLevel.Unsafe;
using Unity.Entities;

public struct PendingAgentActions : ICleanupComponentData
{
    public UnsafeLinkedPriorityQueue<AgentAction, ActionPriorityComparer> Queue;
}