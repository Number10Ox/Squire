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
        var squireQuery = SystemAPI.QueryBuilder().WithAll<SquireTag>().Build();
        if (squireQuery.IsEmpty)
        {
            var squirePrefab = SystemAPI.GetSingleton<Spawner>().SquirePrefab;
            SpawnCharacter(ref state, squirePrefab);
        }
        var heroQuery = SystemAPI.QueryBuilder().WithAll<HeroTag>().Build();
        if (heroQuery.IsEmpty)
        {
            var heroPrefab = SystemAPI.GetSingleton<Spawner>().HeroPrefab;
            SpawnCharacter(ref state, heroPrefab);
        }
    }

    private void SpawnCharacter(ref SystemState state, Entity characterPrefab)
    {
        // Instantiating an entity creates copy entities with the same component types and values.
        var instances = state.EntityManager.Instantiate(characterPrefab, 1, Allocator.Temp);
            
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