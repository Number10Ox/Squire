using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup)), UpdateAfter(typeof(PlayerInputSystem))]
public partial struct SquireControlSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<SquireTag>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (targetPosition, targetEntity, entity) in SystemAPI.Query<
                         RefRO<TargetPosition>,
                         RefRO<TargetEntity>>()
                     .WithAll<SquireTag>()
                     .WithEntityAccess())
        {
            if (targetPosition.ValueRO.IsSet)
            {
                MoveToPosition(ref state, entity);
            }
            else if (targetEntity.ValueRO.IsSet)
            {
                MoveToAndInteractWithEntity(ref state, entity);
            }
        }
    }

    void MoveToPosition(ref SystemState state, Entity squireEntity)
    {
        var targetPosition = SystemAPI.GetComponent<TargetPosition>(squireEntity);

        var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Debug.LogFormat("Creating MoveTo POSITION action to position {0}", TargetPosition.TargetPosition);

        // Create the action entity
        var actionEntity = ecb.CreateEntity();
        ecb.AddComponent(actionEntity, new AgentMoveToPositionAction
        {
            TargetPosition = targetPosition.Position
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

        ecb.SetComponent(squireEntity, new TargetPosition()
        {
            Position = float3.zero,
            IsSet = false
        });
    }

    private void MoveToAndInteractWithEntity(ref SystemState state, Entity squireEntity)
    {
        var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var targetEntity = SystemAPI.GetComponent<TargetEntity>(squireEntity);
        var targetEntityPosition = SystemAPI.GetComponent<LocalTransform>(targetEntity.Target);

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
        ecb.AddComponent(moveToActionEntity,
            new AgentMoveToPositionAction() { TargetPosition = targetEntityPosition.Position });
        ecb.AddComponent(moveToActionEntity, new AgentAction()
        {
            Type = AgentActionType.MoveTo,
            Priority = 1,
            Interrupt = true,
            State = AgentActionState.NotStarted,
            Result = AgentActionResult.Pending
        });

        Entity interactActionEntity = ecb.CreateEntity();
        ecb.AddComponent(interactActionEntity, new AgentInteractAction { TargetEntity = targetEntity.Target });
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
        ecb.AppendToBuffer(actionSequenceEntity,
            new AgentSequenceActionElement { ActionEntity = interactActionEntity });

        // Add the sequence action to the squire's pending action queue
        ecb.AppendToBuffer(squireEntity, new AgentPendingActionElement()
        {
            ActionEntity = actionSequenceEntity
        });

        // Disable player TargetEntity component
        ecb.SetComponent(squireEntity, new TargetEntity
        {
            Target = Entity.Null,
            IsSet = false
        });
    }
}