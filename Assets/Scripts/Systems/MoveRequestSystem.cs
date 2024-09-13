using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(MoveRequestInitSystem))]
[BurstCompile]
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
    
    [BurstCompile]
    protected override void OnUpdate()
    {
        // Get the singleton entity containing the dynamic buffer
        var singletonEntity = SystemAPI.GetSingletonEntity<MoveRequestBufferElement>();

        var moveRequestBuffer = EntityManager.GetBuffer<MoveRequestBufferElement>(singletonEntity);

        // Process each movement request in the buffer
        foreach (var moveRequest in moveRequestBuffer)
        {
            float3 targetPosition = moveRequest.TargetPosition;
            
            foreach (var (destination, body) in SystemAPI.Query<RefRO<SquireTag>, RefRW<AgentBody>>())
            {
                body.ValueRW.SetDestination(targetPosition);
            } 
        }

        // Clear the buffer after processing all movement requests
        moveRequestBuffer.Clear();
    }
}