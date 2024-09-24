using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct AgentActionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AgentTag>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (agentActions, entity) in
                 SystemAPI.Query<DynamicBuffer<AgentAction>>()
                     .WithAll<AgentTag>()
                     .WithEntityAccess())
        {
            if (agentActions.Length > 0)
            {
                Debug.LogFormat("Agent action queue size {0}", agentActions.Length);
                agentActions.RemoveAt(0);
            }
        }
    }
}