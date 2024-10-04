using Unity.Entities;
using ProjectDawn.Navigation;

public abstract partial class AgentActionSystemBase : SystemBase
{
    protected EntityCommandBuffer ecb;

    protected override void OnCreate()
    {
        RequireForUpdate<AgentTag>();
        RequireForUpdate<AgentBody>();
    }

    protected override void OnUpdate()
    {
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        Entities
            .WithAll<AgentTag>()
            .ForEach((Entity entity, ref DynamicBuffer<AgentActiveActionData> activeActions, ref AgentBody agentBody, in AgentActiveActionType activeTypes) =>
            {
                if (ShouldProcessActions(activeTypes))
                {
                    ProcessActions(activeActions, ref agentBody);
                }
            }).WithoutBurst().Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }

    protected abstract bool ShouldProcessActions(AgentActiveActionType activeTypes);

    protected abstract void ProcessSpecificAction(Entity actionEntity, ref AgentAction actionData, ref AgentBody agentBody);

    private void ProcessActions(DynamicBuffer<AgentActiveActionData> activeActions, ref AgentBody agentBody)
    {
        for (int i = 0; i < activeActions.Length; i++)
        {
            var actionEntity = activeActions[i].ActionEntity;
            var actionData = SystemAPI.GetComponent<AgentAction>(actionEntity);

            if (IsTargetActionType(actionEntity))
            {
                ProcessSpecificAction(actionEntity, ref actionData, ref agentBody);
            }
            else if (SystemAPI.HasComponent<AgentActionSequenceAction>(actionEntity))
            {
                ProcessActionSequence(actionEntity, ref actionData, ref agentBody);
            }
        }
    }

    protected abstract bool IsTargetActionType(Entity actionEntity);

    private void ProcessActionSequence(Entity actionEntity, ref AgentAction actionData, ref AgentBody agentBody)
    {
        if (actionData.State != AgentActionState.Running)
        {
            return;
        }

        var buffer = SystemAPI.GetBuffer<AgentSequenceActionData>(actionEntity);
        if (buffer.Length == 0)
        {
            return;
        }

        var activeActionData = buffer[0];
        var runningActionData = SystemAPI.GetComponent<AgentAction>(activeActionData.ActionEntity);

        if (IsTargetActionType(activeActionData.ActionEntity) && runningActionData.State == AgentActionState.NotStarted)
        {
            ProcessSpecificAction(activeActionData.ActionEntity, ref runningActionData, ref agentBody);
        }
    }
}