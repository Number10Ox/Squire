using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(8)] // Adjust based on expected usage
public struct MoveRequestBufferElement : IBufferElementData
{
    public float3 TargetPosition;
}

[BurstCompile]
public partial class MoveRequestInitSystem : SystemBase
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // We need to wait for the scene to load before Updating, so we must RequireForUpdate at
        // least one component type loaded from the scene.
        state.RequireForUpdate<ExecuteDungeonSync>();
    }

    protected override void OnUpdate()
    {
        Enabled = false;
        
        // Check if the singleton already exists
        if (!SystemAPI.HasSingleton<MoveRequestBufferElement>())
        {
            var singletonEntity = EntityManager.CreateEntity();
            EntityManager.AddBuffer<MoveRequestBufferElement>(singletonEntity);
        }
    }
}