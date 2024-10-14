using Unity.Entities;
using ProjectDawn.Navigation;
using Rukhanka;
using Unity.Burst;

[UpdateInGroup(typeof(ActionProcessingSystemGroup))]
public partial struct MoveToActionSystem : ISystem
{
    private FastAnimatorParameter isWalkingParam;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AgentTag>();
        state.RequireForUpdate<AgentBody>();

        isWalkingParam = new FastAnimatorParameter("isWalkingForward");
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (activeActions, agentBody, activeTypes, animationParameters, entity) in
                 SystemAPI
                     .Query<DynamicBuffer<AgentActiveActionElement>, RefRW<AgentBody>, RefRO<AgentActiveActionType>,
                         AnimatorParametersAspect>()
                     .WithAll<AgentTag>()
                     .WithEntityAccess())
        {
            if (activeTypes.ValueRO.Has(AgentActionType.MoveTo) || activeTypes.ValueRO.Has(AgentActionType.Sequence))
            {
                ProcessMoveToActions(activeActions, animationParameters, ref agentBody.ValueRW, ref state, ecb);
            }
        }
    }

    private void ProcessMoveToActions(
        DynamicBuffer<AgentActiveActionElement> activeActions, AnimatorParametersAspect animationParameters,
        ref AgentBody agentBody, ref SystemState state,
        EntityCommandBuffer ecb)
    {
        for (int i = 0; i < activeActions.Length; i++)
        {
            var actionEntity = activeActions[i].ActionEntity;
            var actionData = SystemAPI.GetComponent<AgentAction>(actionEntity);

            if (SystemAPI.HasComponent<AgentMoveToPositionAction>(actionEntity))
            {
                ProcessMoveToPosition(actionEntity, ref actionData, animationParameters, ref agentBody, ref state, ecb);
            }
            else if (SystemAPI.HasComponent<AgentActionSequenceAction>(actionEntity))
            {
                ProcessActionSequenceAction(actionEntity, ref actionData, animationParameters, ref agentBody, ref state,
                    ecb);
            }
        }
    }

    private void ProcessMoveToPosition(Entity actionEntity, ref AgentAction actionData,
        AnimatorParametersAspect animationParameters,
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

                animationParameters.SetBoolParameter(isWalkingParam, true);

                break;
            case AgentActionState.Running:
                if (agentBody.IsStopped)
                {
                    actionData.State = AgentActionState.Done;
                    actionData.Result = AgentActionResult.Success;
                    ecb.SetComponent(actionEntity, actionData);

                    animationParameters.SetBoolParameter(isWalkingParam, false);
                }

                break;
            case AgentActionState.Done:
                // Will be handled by AgentActionSystem
                break;
        }
    }

    private void ProcessActionSequenceAction(Entity actionEntity, ref AgentAction actionData,
        AnimatorParametersAspect animationParameters,
        ref AgentBody agentBody, ref SystemState state, EntityCommandBuffer ecb)
    {
        if (actionData.State != AgentActionState.Running)
        {
            // Wait until sequence action has been processed
            return;
        }

        var buffer = SystemAPI.GetBuffer<AgentSequenceActionElement>(actionEntity);
        var activeActionData = buffer[0];
        var runningActionData = SystemAPI.GetComponent<AgentAction>(activeActionData.ActionEntity);

        if (runningActionData.Type != AgentActionType.MoveTo)
        {
            // Ignore if active action isn't a MoveTo action
            return;
        }

        ProcessMoveToPosition(activeActionData.ActionEntity, ref runningActionData, animationParameters, ref agentBody,
            ref state, ecb);
    }
}