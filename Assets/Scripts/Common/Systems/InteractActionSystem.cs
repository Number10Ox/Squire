using Unity.Entities;
using ProjectDawn.Navigation;
using UnityEngine;

[UpdateInGroup(typeof(ActionProcessingSystemGroup))]
public partial struct InteractActionSystem : ISystem
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

        foreach (var (activeActions, agentBody, activeTypes, entity) in
                 SystemAPI
                     .Query<DynamicBuffer<AgentActiveActionData>, RefRW<AgentBody>, RefRO<AgentActiveActionType>>()
                     .WithAll<AgentTag>()
                     .WithEntityAccess())
        {
            if (activeTypes.ValueRO.Has(AgentActionType.Interact) || activeTypes.ValueRO.Has(AgentActionType.Sequence))
            {
                ProcessInteractActions(activeActions, ref agentBody.ValueRW, ref state, ecb);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void ProcessInteractActions(
        DynamicBuffer<AgentActiveActionData> activeActions, ref AgentBody agentBody, ref SystemState state,
        EntityCommandBuffer ecb)
    {
        for (int i = 0; i < activeActions.Length; i++)
        {
            var actionEntity = activeActions[i].ActionEntity;
            var actionData = SystemAPI.GetComponent<AgentAction>(actionEntity);

            if (SystemAPI.HasComponent<AgentInteractAction>(actionEntity))
            {
                ProcessInteract(actionEntity, ref actionData, ref agentBody, ref state, ecb);
            }
            else if (SystemAPI.HasComponent<AgentActionSequenceAction>(actionEntity))
            {
                ProcessActionSequenceAction(actionEntity, ref actionData, ref agentBody, ref state, ecb);
            }
        }
    }

    private void ProcessInteract(Entity actionEntity, ref AgentAction actionData,
        ref AgentBody agentBody, ref SystemState state, EntityCommandBuffer ecb)
    {
        var interactAction = SystemAPI.GetComponent<AgentInteractAction>(actionEntity);

        switch (actionData.State)
        {
            case AgentActionState.NotStarted:
                Debug.Log($"EXECUTE ACTION: INTERACT on entity {interactAction.TargetEntity}");
            
                ecb.DestroyEntity(interactAction.TargetEntity);

                actionData.State = AgentActionState.Done;
                actionData.Result = AgentActionResult.Success;
                ecb.SetComponent(actionEntity, actionData);
                break;
            case AgentActionState.Running:
                // TODO
                break;
            case AgentActionState.Done:
                // Will be handled by AgentActionSystem
                break;
        }
    }

    private void ProcessActionSequenceAction(Entity actionEntity, ref AgentAction actionData,
        ref AgentBody agentBody, ref SystemState state, EntityCommandBuffer ecb)
    {
        if (actionData.State != AgentActionState.Running)
        {
            // Wait until sequence action has been processed
            return;
        }

        var buffer = SystemAPI.GetBuffer<AgentSequenceActionData>(actionEntity);
        var activeActionData = buffer[0];
        var runningActionData = SystemAPI.GetComponent<AgentAction>(activeActionData.ActionEntity);

        if (runningActionData.Type != AgentActionType.Interact || runningActionData.State != AgentActionState.NotStarted)
        {
            // Ignore if active action isn't an Interact action
            return;
        }

        ProcessInteract(activeActionData.ActionEntity, ref runningActionData, ref agentBody, ref state, ecb);
    }
}