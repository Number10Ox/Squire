using ProjectDawn.Navigation;
using Unity.Entities;

[UpdateInGroup(typeof(ActionProcessingSystemGroup))]
public partial class InteractActionSystem : AgentActionSystemBase
{
    protected override bool ShouldProcessActions(AgentActiveActionType activeTypes)
    {
        return activeTypes.Has(AgentActionType.Interact) || activeTypes.Has(AgentActionType.Sequence);
    }

    protected override bool IsTargetActionType(Entity actionEntity)
    {
        return SystemAPI.HasComponent<AgentInteractAction>(actionEntity);
    }

    protected override void ProcessSpecificAction(Entity actionEntity, ref AgentAction actionData, ref AgentBody agentBody)
    {
        var interactAction = SystemAPI.GetComponent<AgentInteractAction>(actionEntity);

        switch (actionData.State)
        {
            case AgentActionState.NotStarted:
                UnityEngine.Debug.Log("EXECUTE ACTION: INTERACT");
                actionData.State = AgentActionState.Done;
                actionData.Result = AgentActionResult.Success;
                ecb.SetComponent(actionEntity, actionData);
                break;
            case AgentActionState.Running:
                // TODO: Implement running state logic if needed
                break;
            case AgentActionState.Done:
                // Will be handled by AgentActionQueueSystem
                break;
        }
    }
}