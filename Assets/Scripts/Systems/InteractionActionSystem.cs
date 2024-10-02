using Unity.Entities;
using ProjectDawn.Navigation;

[UpdateInGroup(typeof(ActionProcessingSystemGroup))]
public partial struct InteractionActionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AgentTag>();
        state.RequireForUpdate<AgentBody>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [Unity.Burst.BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

    }
}