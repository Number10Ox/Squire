using Unity.Entities;
using ProjectDawn.Navigation;
using Unity.Burst;
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

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (activeActions, agentBody, activeTypes, entity) in
                 SystemAPI
                     .Query<DynamicBuffer<AgentActiveActionData>, RefRW<AgentBody>, RefRO<AgentActiveActionType>>()
                     .WithAll<AgentTag>()
                     .WithEntityAccess())
        {
            if (activeTypes.ValueRO.Has(AgentActionType.MoveTo) || activeTypes.ValueRO.Has(AgentActionType.Sequence))
            {
                ProcessMoveToActions(activeActions, ref agentBody.ValueRW, ref state, ecb);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void ProcessMoveToActions(
        DynamicBuffer<AgentActiveActionData> activeActions, ref AgentBody agentBody, ref SystemState state,
        EntityCommandBuffer ecb)
    {
        for (int i = 0; i < activeActions.Length; i++)
        {
            var actionEntity = activeActions[i].ActionEntity;
            var actionData = SystemAPI.GetComponent<AgentAction>(actionEntity);

            if (SystemAPI.HasComponent<AgentMoveToPositionAction>(actionEntity))
            {
                ProcessMoveToPosition(actionEntity, ref actionData, ref agentBody, ref state, ecb);
            }
            else if (SystemAPI.HasComponent<AgentActionSequenceAction>(actionEntity))
            {
                ProcessActionSequenceAction(actionEntity, ref actionData, ref agentBody, ref state, ecb);
            }
        }
    }

    private void ProcessMoveToPosition(Entity actionEntity, ref AgentAction actionData,
        ref AgentBody agentBody, ref SystemState state, EntityCommandBuffer ecb)
    {
        var moveToAction = SystemAPI.GetComponent<AgentMoveToPositionAction>(actionEntity);

        switch (actionData.State)
        {
            case AgentActionState.NotStarted:
                agentBody.SetDestination(moveToAction.TargetPosition);
                // Debug.Log("--> SETTING DESTINATION");
                actionData.State = AgentActionState.Running;
                ecb.SetComponent(actionEntity, actionData);
                break;
            case AgentActionState.Running:
                if (agentBody.IsStopped)
                {
                    actionData.State = AgentActionState.Done;
                    actionData.Result = AgentActionResult.Success;
                    ecb.SetComponent(actionEntity, actionData);
                }

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

        if (runningActionData.Type != AgentActionType.MoveTo) 
        {
            // Ignore if active action isn't a MoveTo action
            return;
        }

        ProcessMoveToPosition(activeActionData.ActionEntity, ref runningActionData, ref agentBody, ref state, ecb);
    }
}

/* TODO WORK IN PROGRESS
 
[UpdateInGroup(typeof(ActionProcessingSystemGroup))]
public partial struct ParallelMoveToActionSystem : ISystem
{
    private EntityQuery actionQuery;
    private ComponentLookup<AgentBody> agentBodyLookup;
    
    public void OnCreate(ref SystemState state)
    {
        actionQuery = state.GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                ComponentType.ReadWrite<AgentAction>(),
                ComponentType.ReadOnly<AgentActiveActionType>(),
                ComponentType.ReadOnly<AgentBody>(),
                ComponentType.ReadOnly<AgentMoveToPositionAction>(),
            },
        });

        agentBodyLookup = state.GetComponentLookup<AgentBody>(false);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
    }
}
*/