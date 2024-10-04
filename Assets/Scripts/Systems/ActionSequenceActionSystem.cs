using System;
using Unity.Entities;
using ProjectDawn.Navigation;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(ActionProcessingSystemGroup))]
public partial struct ActionSequenceActionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AgentTag>();
        state.RequireForUpdate<AgentBody>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [Unity.Burst.BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (activeActions, agentBody, activeActionTypes, entity) in
                 SystemAPI
                     .Query<DynamicBuffer<AgentActiveActionData>, RefRW<AgentBody>, RefRO<AgentActiveActionType>>()
                     .WithAll<AgentTag>()
                     .WithEntityAccess())
        {
            if (activeActionTypes.ValueRO.Has(AgentActionType.Sequence))
            {
                ProcessActionSequenceActions(activeActions, ref agentBody.ValueRW, ref state, ecb);
            }
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    // Agent has a sequence action among its active actions
    private void ProcessActionSequenceActions(DynamicBuffer<AgentActiveActionData> activeActions,
        ref AgentBody agentBody, ref SystemState state, EntityCommandBuffer ecb)
    {
        for (int i = 0; i < activeActions.Length; i++)
        {
            var actionEntity = activeActions[i].ActionEntity;
            if (SystemAPI.HasComponent<AgentActionSequenceAction>(actionEntity))
            {
                var actionData = SystemAPI.GetComponent<AgentAction>(actionEntity);
                ProcessActionSequenceAction(actionEntity, ref actionData, ref agentBody, ref state, ecb);

                break;
            }
        }
    }

    private void ProcessActionSequenceAction(Entity actionEntity, ref AgentAction actionData,
        ref AgentBody agentBody, ref SystemState state, EntityCommandBuffer ecb)
    {
        switch (actionData.State)
        {
            case AgentActionState.NotStarted:
                UpdateCurrentActionInSequence(actionEntity, ref actionData, ref agentBody, ref state, ecb);

                actionData.State = AgentActionState.Running;
                ecb.SetComponent(actionEntity, actionData);
                break;
            case AgentActionState.Running:
                UpdateCurrentActionInSequence(actionEntity, ref actionData, ref agentBody, ref state, ecb);
                break;
            case AgentActionState.Done:
                // Will be handled by AgentActionQueueSystem
                break;
        }
    }

    private void UpdateCurrentActionInSequence(Entity actionEntity, ref AgentAction actionData,
        ref AgentBody agentBody, ref SystemState state, EntityCommandBuffer ecb)
    {
        var buffer = SystemAPI.GetBuffer<AgentSequenceActionData>(actionEntity);

        if (buffer.IsEmpty)
        {
            CompleteAction(ref actionData, actionEntity, ecb);
            return;
        }

        var activeActionData = buffer[0];
        var runningAction = SystemAPI.GetComponent<AgentAction>(activeActionData.ActionEntity);

        if (runningAction.State != AgentActionState.Done)
        {
            return;
        }

        switch (runningAction.Result)
        {
            case AgentActionResult.Success:
                var newBuffer = new NativeArray<AgentSequenceActionData>(buffer.Length - 1, Allocator.Temp);
                for (int i = 1; i < buffer.Length; i++)
                {
                    newBuffer[i - 1] = buffer[i];
                }

                ecb.SetBuffer<AgentSequenceActionData>(actionEntity).CopyFrom(newBuffer);
                newBuffer.Dispose();
                
                if (buffer.Length == 1) // Will be empty after removal
                {
                    CompleteAction(ref actionData, actionEntity, ecb);
                }

                break;
            case AgentActionResult.Fail:
                CompleteAction(ref actionData, actionEntity, ecb);
                break;
            default:
                throw new InvalidOperationException("Action is complete but result is still pending");
        }
    }

    private void CompleteAction(ref AgentAction actionData, Entity actionEntity, EntityCommandBuffer ecb)
    {
        actionData.State = AgentActionState.Done;
        ecb.SetComponent(actionEntity, actionData);
    }
}