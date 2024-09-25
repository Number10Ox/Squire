using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(AgentActionSystem))]
public partial class ActionProcessingSystemGroup : ComponentSystemGroup
{
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct AgentActionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AgentTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (pendingActions, activeActions, activeTypes, entity) in
                 SystemAPI
                     .Query<DynamicBuffer<PendingAgentAction>, DynamicBuffer<ActiveAgentAction>,
                         RefRW<ActiveAgentActionTypes>>()
                     .WithAll<AgentTag>()
                     .WithEntityAccess())
        {
            SortPendingActionsByPriority(ref state, pendingActions);
            RemoveCompletedActions(ref state, activeActions, ref activeTypes.ValueRW, ecb);
            ProcessPendingActions(ref state, pendingActions, activeActions, ref activeTypes.ValueRW, ecb);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private void SortPendingActionsByPriority(ref SystemState state, DynamicBuffer<PendingAgentAction> pendingActions)
    {
        var actionDataLookup = SystemAPI.GetComponentLookup<AgentAction>(true);
        pendingActions.AsNativeArray().Sort(new ActionPriorityComparer(actionDataLookup));
    }

    private void RemoveCompletedActions(ref SystemState state,
        DynamicBuffer<ActiveAgentAction> activeActions,
        ref ActiveAgentActionTypes activeTypes,
        EntityCommandBuffer ecb)
    {
        for (int i = activeActions.Length - 1; i >= 0; i--)
        {
            var actionEntity = activeActions[i].ActionEntity;
            var agentAction = SystemAPI.GetComponent<AgentAction>(actionEntity);

            if (agentAction.State == AgentActionState.Done)
            {
                Debug.Log("Active action is DONE");
                activeActions.RemoveAt(i);
                activeTypes.Remove(agentAction.Type);
                ecb.DestroyEntity(actionEntity);
            }
        }
    }

    private void ProcessPendingActions(ref SystemState state,
        DynamicBuffer<PendingAgentAction> pendingActions,
        DynamicBuffer<ActiveAgentAction> activeActions,
        ref ActiveAgentActionTypes activeTypes,
        EntityCommandBuffer ecb)
    {
        int highestActivePriority = GetHighestActivePriority(ref state, activeActions);

        for (int i = 0; i < pendingActions.Length; i++)
        {
            var pendingActionEntity = pendingActions[i].ActionEntity;
            var pendingActionData = SystemAPI.GetComponent<AgentAction>(pendingActionEntity);

            if (pendingActionData.Priority < highestActivePriority)
            {
                Debug.Log("Stopping Processing pending actions because priority is lower than highest");
                break; // Stop processing as remaining actions have lower priority
            }

            Debug.LogFormat("Processing pending action");

            if (pendingActionData.Interrupt)
            {
                Debug.Log("Pending action INTERRUPTING");
                // Clear active actions and activate this one
                ClearActiveActions(ref state, activeActions, ref activeTypes, ecb);
                ActivateAction(ref state, pendingActionEntity, pendingActionData, activeActions, ref activeTypes, ecb);
                highestActivePriority = pendingActionData.Priority;
            }
            else if (CanRunInParallelWithAll(ref state, pendingActionData, activeActions))
            {
                // Activate this action alongside existing ones
                ActivateAction(ref state, pendingActionEntity, pendingActionData, activeActions, ref activeTypes, ecb);
            }

            // Remove the processed pending action
            pendingActions.RemoveAt(i);
            i--; // Adjust index after removal
        }
    }

    private int GetHighestActivePriority(ref SystemState state, DynamicBuffer<ActiveAgentAction> activeActions)
    {
        int highestPriority = int.MinValue;
        foreach (var activeAction in activeActions)
        {
            var actionData = SystemAPI.GetComponent<AgentAction>(activeAction.ActionEntity);
            if (actionData.Priority > highestPriority)
            {
                highestPriority = actionData.Priority;
            }
        }

        return highestPriority;
    }

    private bool CanRunInParallelWithAll(ref SystemState state, AgentAction pendingAction,
        DynamicBuffer<ActiveAgentAction> activeActions)
    {
        foreach (var activeAction in activeActions)
        {
            var activeActionData = SystemAPI.GetComponent<AgentAction>(activeAction.ActionEntity);
            if (!CanRunInParallel(pendingAction.Type, activeActionData.Type))
            {
                return false;
            }
        }

        return true;
    }

    private bool CanRunInParallel(AgentActionType type1, AgentActionType type2)
    {
        // For now, MoveToAction cannot run in parallel with anything
        if (type1 == AgentActionType.MoveTo || type2 == AgentActionType.MoveTo)
        {
            return false;
        }

        // Add more rules for other action types as needed
        return true;
    }

    private void ClearActiveActions(ref SystemState state,
        DynamicBuffer<ActiveAgentAction> activeActions,
        ref ActiveAgentActionTypes activeTypes,
        EntityCommandBuffer ecb)
    {
        for (int i = activeActions.Length - 1; i >= 0; i--)
        {
            var actionEntity = activeActions[i].ActionEntity;
            var actionData = SystemAPI.GetComponent<AgentAction>(actionEntity);
            actionData.State = AgentActionState.Done;
            ecb.SetComponent(actionEntity, actionData);
        }

        activeActions.Clear();
        activeTypes.Clear();
    }

    private void ActivateAction(ref SystemState state,
        Entity actionEntity,
        AgentAction actionData,
        DynamicBuffer<ActiveAgentAction> activeActions,
        ref ActiveAgentActionTypes activeTypes,
        EntityCommandBuffer ecb)
    {
        Debug.LogFormat("Activating action of type {0}", actionData.Type);

        activeActions.Add(new ActiveAgentAction { ActionEntity = actionEntity });
        activeTypes.Add(actionData.Type);
        actionData.State = AgentActionState.NotStarted;
        ecb.SetComponent(actionEntity, actionData);
    }

    private struct ActionPriorityComparer : IComparer<PendingAgentAction>
    {
        [ReadOnly]
        private ComponentLookup<AgentAction> agentActionFromEntity;

        public ActionPriorityComparer(ComponentLookup<AgentAction> actionDataFromEntity)
        {
            this.agentActionFromEntity = actionDataFromEntity;
        }

        public int Compare(PendingAgentAction x, PendingAgentAction y)
        {
            return agentActionFromEntity[y.ActionEntity].Priority
                .CompareTo(agentActionFromEntity[x.ActionEntity].Priority);
        }
    }
}