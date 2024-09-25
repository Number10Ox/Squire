using Unity.Entities;
using Unity.Mathematics;
using ProjectDawn.Navigation;

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

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (activeActions, agentBody, entity) in 
            SystemAPI.Query<DynamicBuffer<ActiveAgentAction>, RefRW<AgentBody>>()
                .WithAll<AgentTag>()
                .WithEntityAccess())
        {
            for (int i = 0; i < activeActions.Length; i++)
            {
                var actionEntity = activeActions[i].ActionEntity;
                
                if (SystemAPI.HasComponent<AgentMoveToPositionAction>(actionEntity))
                {
                    var actionData = SystemAPI.GetComponent<AgentAction>(actionEntity);
                    var moveToAction = SystemAPI.GetComponent<AgentMoveToPositionAction>(actionEntity);

                    switch (actionData.State)
                    {
                        case AgentActionState.NotStarted:
                            // Start the move
                            agentBody.ValueRW.SetDestination(moveToAction.TargetPosition);
                            actionData.State = AgentActionState.Running;
                            ecb.SetComponent(actionEntity, actionData);
                            break;

                        case AgentActionState.Running:
                            if (agentBody.ValueRO.IsStopped)
                            {
                                // Movement completed
                                actionData.State = AgentActionState.Done;
                                ecb.SetComponent(actionEntity, actionData);
                            }
                            else if (actionData.HasBeenInterrupted)
                            {
                                // Handle interruption
                                agentBody.ValueRW.Stop();
                                actionData.State = AgentActionState.Done;
                                ecb.SetComponent(actionEntity, actionData);
                            }
                            break;

                        case AgentActionState.Done:
                            // Action is done, it will be removed by AgentActionSystem
                            break;
                    }
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}