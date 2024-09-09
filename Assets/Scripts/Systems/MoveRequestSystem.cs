using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(MoveRequestInitSystem))]
public partial class ProcessMoveRequestsSystem : SystemBase
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // We need to wait for the scene to load before Updating, so we must RequireForUpdate at
        // least one component type loaded from the scene.
        state.RequireForUpdate<ExecuteDungeonSync>();
        state.RequireForUpdate<SquireTag>();
    }
    
    protected override void OnUpdate()
    {
        // Get the singleton entity containing the dynamic buffer
        var singletonEntity = GetSingletonEntity<MoveRequestBufferElement>();

        var moveRequestBuffer = EntityManager.GetBuffer<MoveRequestBufferElement>(singletonEntity);

        // Process each movement request in the buffer
        foreach (var moveRequest in moveRequestBuffer)
        {
            float3 targetPosition = moveRequest.TargetPosition;
            UpdateSquireTargetPosition(targetPosition);
        }

        // Clear the buffer after processing all movement requests
        moveRequestBuffer.Clear();
    }

    private void UpdateSquireTargetPosition(float3 targetPosition)
    {
        var query = GetEntityQuery(ComponentType.ReadOnly<SquireTag>(), ComponentType.ReadWrite<AgentBody>());
        if (query.CalculateEntityCount() == 1)
        {
            // Set the destination directly without parallelization for now
            Entities
                .WithAll<SquireTag>() // Ensure you're targeting the Squire
                .ForEach((ref AgentBody body) =>
                {
                    Debug.Log("ProcessMoveRequestsSystem: Setting Destination to " + targetPosition);
                    body.Destination = targetPosition;
                }).Run(); // Use Run() instead of ScheduleParallel() to avoid issues with job dependencies
        }
        else
        {
            Debug.LogError("ProcessMoveRequestsSystem: Could not find exactly one Squire entity.");
        }
    }
}