using Unity.Entities;
using ProjectDawn.Navigation;
using UnityEngine;

[UpdateInGroup(typeof(ActionProcessingSystemGroup))]
public partial struct MoveToActionSystem : ISystem
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
            SystemAPI.Query<DynamicBuffer<ActiveAgentAction>, RefRW<AgentBody>, RefRO<ActiveAgentActionTypes>>()
                .WithAll<AgentTag>()
                .WithEntityAccess())
        {
            // Skip agents without active MoveToActions
            if (!activeTypes.ValueRO.Has(AgentActionType.MoveTo)) 
                continue; 

            ProcessMoveToActions(activeActions, ref agentBody.ValueRW, entity, ref state, ecb);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void ProcessMoveToActions(
        DynamicBuffer<ActiveAgentAction> activeActions, 
        ref AgentBody agentBody,
        Entity agentEntity,
        ref SystemState state,
        EntityCommandBuffer ecb)
    {
        for (int i = 0; i < activeActions.Length; i++)
        {
            var actionEntity = activeActions[i].ActionEntity;
            
            if (!SystemAPI.HasComponent<AgentMoveToPositionAction>(actionEntity)) continue;

            var actionData = SystemAPI.GetComponent<AgentAction>(actionEntity);
            var moveToAction = SystemAPI.GetComponent<AgentMoveToPositionAction>(actionEntity);

            switch (actionData.State)
            {
                case AgentActionState.NotStarted:
                    Debug.Log("STARTING: MoveTo");
                    agentBody.SetDestination(moveToAction.TargetPosition);
                    actionData.State = AgentActionState.Running;
                    ecb.SetComponent(actionEntity, actionData);
                    break;
                case AgentActionState.Running:
                    if (agentBody.IsStopped)
                    {
                        Debug.Log("MARKING DONE: MoveTo");
                        actionData.State = AgentActionState.Done;
                        ecb.SetComponent(actionEntity, actionData);
                    }
                    break;
                case AgentActionState.Done:
                    // Will be handled by AgentActionSystem
                    break;
            }
        }
    }
}