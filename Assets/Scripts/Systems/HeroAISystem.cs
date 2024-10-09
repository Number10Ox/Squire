
using Unity.Burst;
using Unity.Entities;

public partial struct HeroAISystem : ISystem
{
    [BurstCompile] 
    void OnUpdate(ref SystemState state)
    {
        
    }

    /*
    public partial struct HeroAIUpdateJob : IJobEntity
    {
         
    }
    */
}