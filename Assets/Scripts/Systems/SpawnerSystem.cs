using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // This call makes the system not update unless at least one entity in the world exists that has the Spawner component.
        state.RequireForUpdate<Spawner>();
        state.RequireForUpdate<ExecuteDungeonSync>();
        state.RequireForUpdate<SquireSpawnPointTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Create a query that matches all entities having a RotationSpeed component.
        // (The query is cached in source generation, so this does not incur a cost of recreating it every update.)
        var squireQuery = SystemAPI.QueryBuilder().WithAll<SquireTag>().Build();

        // Only spawn cubes when no cubes currently exist.
        if (squireQuery.IsEmpty)
        {
            var squirePrefab = SystemAPI.GetSingleton<Spawner>().SquirePrefab;
        
            // Instantiating an entity creates copy entities with the same component types and values.
            var instances = state.EntityManager.Instantiate(squirePrefab, 1, Allocator.Temp);
            
            var squireSpawnPointEntity = SystemAPI.GetSingletonEntity<SquireSpawnPointTag>();
            SpawnPoint spawnPoint = SystemAPI.GetComponent<SpawnPoint>(squireSpawnPointEntity);

            foreach (var entity in instances)
            {
                // Position the squire
                var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
                transform.ValueRW.Position = spawnPoint.Position;
                // Debug.LogFormat("Spawning squire at {0}", spawnPoint.Position);
            }
        }
    }
}