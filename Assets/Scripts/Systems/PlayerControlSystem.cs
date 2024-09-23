using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PlayerControlSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        // Check for has TargetPosition component
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        if (SystemAPI.HasComponent<TargetPosition>(playerEntity))
        {
            var targetPosition = SystemAPI.GetComponent<TargetPosition>(playerEntity);
            // TODONOW Create a MoveToPosition action entity and add it to action queue for squire
        }
    }
}
