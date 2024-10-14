using Unity.Entities;
using Unity.Transforms;
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
        if (SystemAPI.IsComponentEnabled<TargetPosition>(playerEntity))
        {
            MoveToPosition(ref state, playerEntity); 
        }
        else if (SystemAPI.IsComponentEnabled<TargetEntity>(playerEntity))
        {
            MoveToAndInteractWithEntity(ref state, playerEntity); 
        } 
    }

    void MoveToPosition(ref SystemState state, Entity playerEntity)
    {
        // TODO Need to find a squire entity for the current player and not except a singleton squire
        var targetPosition = SystemAPI.GetComponent<TargetPosition>(playerEntity);
        var squireEntity = SystemAPI.GetSingletonEntity<SquireTag>();
        
        var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Debug.LogFormat("Creating MoveTo POSITION action to position {0}", targetPosition.targetPosition);
        
        // Create the action entity
        var actionEntity = ecb.CreateEntity();
        ecb.AddComponent(actionEntity, new AgentMoveToPositionAction
        {
            TargetPosition = targetPosition.targetPosition
        });

        ecb.AddComponent(actionEntity, new AgentAction()
        {
            Type = AgentActionType.MoveTo,
            Priority = 1,
            Interrupt = true,
            State = AgentActionState.NotStarted,
            Result = AgentActionResult.Pending
        }); 
        
        ecb.AppendToBuffer(squireEntity, new AgentPendingActionElement()
        {
            ActionEntity = actionEntity
        });

        // Disable player TargetPosition component
        ecb.SetComponentEnabled<TargetPosition>(playerEntity, false);
    }

    private void MoveToAndInteractWithEntity(ref SystemState state, Entity playerEntity)
    {
        var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        
        // TODO Need to find a squire entity for the current player and not except a singleton squire
        var targetEntity = SystemAPI.GetComponent<TargetEntity>(playerEntity);
        var squireEntity = SystemAPI.GetSingletonEntity<SquireTag>();

        var targetEntityPosition = SystemAPI.GetComponent<LocalTransform>(targetEntity.target);
        
        // Create an action entity with AgentAction and AgentActionSequenceAction components
        // and a queue of AgentPendingActionElement 
        var actionSequenceEntity = ecb.CreateEntity();
        
        ecb.AddComponent(actionSequenceEntity, new AgentAction()
        {
            Type = AgentActionType.Sequence,
            Priority = 1,
            Interrupt = true,
            State = AgentActionState.NotStarted,
            Result = AgentActionResult.Pending
        });  
        
        ecb.AddComponent(actionSequenceEntity, new AgentActionSequenceAction());
        ecb.AddBuffer<AgentSequenceActionElement>(actionSequenceEntity);

        Debug.LogFormat("Creating MoveTo ENTITY action to position {0}", targetEntityPosition.Position);
        
        // Create MoveTo and Interact actions
        Entity moveToActionEntity = ecb.CreateEntity();
        ecb.AddComponent(moveToActionEntity, new AgentMoveToPositionAction() { TargetPosition =  targetEntityPosition.Position  });
        ecb.AddComponent(moveToActionEntity, new AgentAction()
        {
            Type = AgentActionType.MoveTo,
            Priority = 1,
            Interrupt = true,
            State = AgentActionState.NotStarted,
            Result = AgentActionResult.Pending
        });    
        
        Entity interactActionEntity = ecb.CreateEntity();
        ecb.AddComponent(interactActionEntity, new AgentInteractAction { TargetEntity = targetEntity.target });
        ecb.AddComponent(interactActionEntity, new AgentAction()
        {
            Type = AgentActionType.Interact,
            Priority = 1,
            Interrupt = true,
            State = AgentActionState.NotStarted,
            Result = AgentActionResult.Pending
        });  

        // Add the MoveTo and Interact action the AgentActionSequenceAction pending action queue
        ecb.AppendToBuffer(actionSequenceEntity, new AgentSequenceActionElement { ActionEntity = moveToActionEntity });
        ecb.AppendToBuffer(actionSequenceEntity, new AgentSequenceActionElement { ActionEntity = interactActionEntity});
        
        // Add the sequence action to the squire's pending action queue
        ecb.AppendToBuffer(squireEntity, new AgentPendingActionElement()
        {
            ActionEntity = actionSequenceEntity
        }); 
            
        // Disable player TargetEntity component
        ecb.SetComponentEnabled<TargetEntity>(playerEntity, false);
    }
}
