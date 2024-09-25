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
        if (!SystemAPI.IsComponentEnabled<TargetPosition>(playerEntity))
            return;
        
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

        ecb.AddComponent(actionEntity, new AgentAction()
        {
            Priority = 1,
            CanInterrupt = true,
            CanRunInParallel = true,
            State = AgentActionState.NotStarted,
            HasBeenInterrupted = false
        }); 
        
        ecb.AppendToBuffer(squireEntity, new PendingAgentAction()
        {
            ActionEntity = actionEntity
        });

        // Disable the TargetPosition component
        ecb.SetComponentEnabled<TargetPosition>(playerEntity, false);
    }
}
