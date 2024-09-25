using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(AgentActionSystem))]
public partial class ActionProcessingSystemGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct AgentActionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (pendingActions, activeActions, activeTypes, entity) in 
            SystemAPI.Query<DynamicBuffer<PendingAgentAction>, DynamicBuffer<ActiveAgentAction>, RefRW<ActiveAgentActionTypes>>()
                .WithAll<AgentTag>()
                .WithEntityAccess())
        {
            // Step 1: Update state of active actions
            for (int i = activeActions.Length - 1; i >= 0; i--)
            {
                var actionEntity = activeActions[i].ActionEntity;
                var actionData = SystemAPI.GetComponent<AgentAction>(actionEntity);

                if (actionData.State == AgentActionState.Done)
                {
                    // Remove completed action
                    activeActions.RemoveAt(i);
                    UpdateActiveTypesMask(ref activeTypes.ValueRW, actionEntity, false);
                    ecb.DestroyEntity(actionEntity);
                }
                else
                {
                    // Update the action data if needed
                    ecb.SetComponent(actionEntity, actionData);
                }
            }

            // Step 2: Process pending actions
            for (int i = pendingActions.Length - 1; i >= 0; i--)
            {
                var pendingActionEntity = pendingActions[i].ActionEntity;
                var pendingActionData = SystemAPI.GetComponent<AgentAction>(pendingActionEntity);

                if (CanActivateAction(pendingActionData, activeActions, activeTypes.ValueRO))
                {
                    // Activate the action
                    activeActions.Add(new ActiveAgentAction { ActionEntity = pendingActionEntity });
                    UpdateActiveTypesMask(ref activeTypes.ValueRW, pendingActionEntity, true);
                    pendingActionData.State = AgentActionState.NotStarted;
                    ecb.SetComponent(pendingActionEntity, pendingActionData);
                    pendingActions.RemoveAt(i);
                }
                else if (ShouldDiscardAction(pendingActionData, activeActions))
                {
                    // Discard the action
                    ecb.DestroyEntity(pendingActionEntity);
                    pendingActions.RemoveAt(i);
                }
                // If neither activated nor discarded, the action remains pending
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private bool CanActivateAction(AgentAction pendingAction, DynamicBuffer<ActiveAgentAction> activeActions, ActiveAgentActionTypes activeTypes)
    {
        if (pendingAction.CanRunInParallel)
        {
            return true; // Can always run parallel actions
        }

        if (activeActions.Length == 0)
        {
            return true; // No active actions, so we can activate this one
        }

        // Check if this action can interrupt others
        if (pendingAction.CanInterrupt)
        {
            // Logic to check if this action should interrupt current actions
            // This might involve comparing priorities or other game-specific logic
            return true; // Placeholder, replace with actual logic
        }

        return false; // Cannot activate if there are active actions and this can't interrupt
    }

    private bool ShouldDiscardAction(AgentAction pendingAction, DynamicBuffer<ActiveAgentAction> activeActions)
    {
        // Implement logic to decide if the action should be discarded
        // This might depend on the current active actions and the pending action's properties
        return false; // Placeholder
    }

    private void UpdateActiveTypesMask(ref ActiveAgentActionTypes activeTypes, Entity actionEntity, bool isActivating)
    {
        // Update the ActiveTypesMask based on the action type
        // You'll need to implement a way to determine the action type from the actionEntity
    }
}