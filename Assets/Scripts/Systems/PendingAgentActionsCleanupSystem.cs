using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PendingAgentActionsCleanupSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<PendingAgentActions>();
    }

    protected override void OnUpdate()
    {
        Entities
            .WithStructuralChanges()
            .ForEach((Entity entity, in PendingAgentActions cleanup) =>
            {
                if (!EntityManager.HasComponent<AgentTag>(entity))
                {
                    // The agent entity has been destroyed or AgentTag has been removed
                    // We need to clean up the NativeLinkedPriorityQueue
                    if (EntityManager.GetComponentData<PendingAgentActions>(entity).Queue.IsCreated)
                    {
                        EntityManager.GetComponentData<PendingAgentActions>(entity).Queue.Dispose();
                    }
                    EntityManager.RemoveComponent<PendingAgentActions>(entity);
                }
            }).Run();
    }
}