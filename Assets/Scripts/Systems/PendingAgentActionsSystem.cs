using ProjectDawn.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PendingAgentActionsSystem : SystemBase
{
    private EntityQuery newAgentsQuery;

    protected override void OnCreate()
    {
        newAgentsQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(AgentTag) },
            None = new ComponentType[] { typeof(PendingAgentActions) }
        });
        RequireForUpdate(newAgentsQuery);
    }

    protected override void OnDestroy()
    {
        // Dispose of the native container during system destruction
        Entities
            .WithStructuralChanges()
            .ForEach((ref PendingAgentActions pendingActions) =>
            {
                if (pendingActions.Queue.IsCreated)
                {
                    pendingActions.Queue.Dispose();
                }
            }).Run();
    }

    protected override void OnUpdate()
    {
        // Create an ActionPriorityComparer instance for the queue
        var comparer = new ActionPriorityComparer();

        // Perform structural changes and add PendingAgentActions and cleanup component
        Entities
            .WithStoreEntityQueryInField(ref newAgentsQuery)
            .WithStructuralChanges()
            .ForEach((Entity entity) =>
            {
                var queue = new UnsafeLinkedPriorityQueue<AgentAction, ActionPriorityComparer>(1, Allocator.Persistent, comparer);

                // Add the PendingAgentActions component to the entity
                EntityManager.AddComponentData(entity, new PendingAgentActions
                {
                    Queue = queue
                });

                // Add a cleanup component to handle the queue lifecycle
                EntityManager.AddComponent<PendingAgentActions>(entity);
            }).Run();
    }
}