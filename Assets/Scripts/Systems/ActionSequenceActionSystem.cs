using Unity.Entities;
using ProjectDawn.Navigation;

[UpdateInGroup(typeof(ActionProcessingSystemGroup))]
public partial struct ActionSequenceActionSystem : ISystem
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
        // An ActionSequence is done when 
        //      an action in the sequence completes and has failed
        //      all actions in the sequence have successfully completed
        // 
        // The systems that process agent agents that are part of a sequence
        // need to know when they can run. I believe they need to check for
        // AgentActionSequenceActions among the active actions of agents
        // in addition to actions of their type.
    }
}