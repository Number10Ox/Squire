using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // This call makes the system not update unless at least one entity in the world exists that has the Spawner component.
        state.RequireForUpdate<Spawner>();
        state.RequireForUpdate<DirectoryManaged>();
        state.RequireForUpdate<ExecuteDungeonSync>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Create a query that matches all entities having a RotationSpeed component.
        // (The query is cached in source generation, so this does not incur a cost of recreating it every update.)
        var squireQuery = SystemAPI.QueryBuilder().WithAll<SquireTag>().Build();

        // Only spawn cubes when no cubes currently exist.
        if (squireQuery.IsEmpty)
        {
            var prefab = SystemAPI.GetSingleton<Spawner>().Prefab;

            // Instantiating an entity creates copy entities with the same component types and values.
            var instances = state.EntityManager.Instantiate(prefab, 1, Allocator.Temp);

            var directory = SystemAPI.ManagedAPI.GetSingleton<DirectoryManaged>();
            var position = directory.SquireSpawnPoint.transform.position;

            foreach (var entity in instances)
            {
                // Position the squire
                var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
                transform.ValueRW.Position = position;
            }
        }
    }
}