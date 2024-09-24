using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup)), UpdateAfter(typeof(PlayerInputSystem))]
public partial struct PlayerControlSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<SquireTag>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var targetPosition = SystemAPI.GetComponent<TargetPosition>(playerEntity);
        var squireEntity = SystemAPI.GetSingletonEntity<SquireTag>();

        var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Create the action entity
        var actionEntity = ecb.CreateEntity();
        ecb.AddComponent(actionEntity, new AgentMoveToPositionAction
        {
            TargetPosition = targetPosition.targetPosition
        });

        // Add the action to the squire's pending actions buffer
        ecb.AppendToBuffer(squireEntity, new AgentAction
        {
            ActionEntity = actionEntity,
            Priority = 1,
            CanInterrupt = true,
            CanRunInParallel = false
        });

        // Disable the TargetPosition component
        ecb.SetComponentEnabled<TargetPosition>(playerEntity, false);
    }
}
