using ProjectDawn.Navigation;
using Unity.Entities;

[UpdateInGroup(typeof(ActionProcessingSystemGroup))]
public partial class MoveToActionSystem : AgentActionSystemBase
{
    protected override bool ShouldProcessActions(AgentActiveActionType activeTypes)
    {
        return activeTypes.Has(AgentActionType.MoveTo) || activeTypes.Has(AgentActionType.Sequence);
    }

    protected override bool IsTargetActionType(Entity actionEntity)
    {
        return SystemAPI.HasComponent<AgentMoveToPositionAction>(actionEntity);
    }

    protected override void ProcessSpecificAction(Entity actionEntity, ref AgentAction actionData, ref AgentBody agentBody)
    {
        var moveToAction = SystemAPI.GetComponent<AgentMoveToPositionAction>(actionEntity);

        switch (actionData.State)
        {
            case AgentActionState.NotStarted:
                agentBody.SetDestination(moveToAction.TargetPosition);
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
                // Will be handled by AgentActionQueueSystem
                break;
        }
    }
}