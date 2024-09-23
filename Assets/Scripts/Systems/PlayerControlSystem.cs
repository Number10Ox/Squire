using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PlayerControlSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<SquireTag>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        // Check for has TargetPosition component
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        if (SystemAPI.IsComponentEnabled<TargetPosition>(playerEntity))
        {
            var targetPosition = SystemAPI.GetComponent<TargetPosition>(playerEntity);
            var squireEntity = SystemAPI.GetSingletonEntity<SquireTag>();
            var squirePendingActions = SystemAPI.GetComponent<PendingAgentActions>(squireEntity);
            
             Debug.LogFormat("PlayerControlSystem: found target position {0}", targetPosition.targetPosition);

            // TODONOW Create a MoveToPosition action entity and add it to action queue for squire
            
            state.EntityManager.SetComponentEnabled<TargetPosition>(playerEntity, false);
            state.EntityManager.SetComponentEnabled<TargetEntity>(playerEntity, false);
        }
    }
}
