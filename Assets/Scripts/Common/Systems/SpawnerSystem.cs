using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnSystem : ISystem
{
    private struct SpawnRequest
    {
        public Entity Prefab;
        public float3 InitialPosition;
        public float Radius;
    }
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Require necessary components for update
        state.RequireForUpdate<Spawner>();
        state.RequireForUpdate<ExecuteDungeonSync>();
        state.RequireForUpdate<SquireSpawnPointTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Create a new random instance with a seed
        var random = new Random((uint)System.DateTime.Now.Ticks);

        // Get the EntityCommandBuffer for the end of initialization group
        var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Initialize the list of spawn requests
        var requests = new NativeList<SpawnRequest>(2, Allocator.Temp);

        // Find spawn point entity and verify it
        var squireSpawnPointEntity = SystemAPI.GetSingletonEntity<SquireSpawnPointTag>();
        var spawnPoint = SystemAPI.GetComponent<SpawnPoint>(squireSpawnPointEntity);
        
        // Check if there are any squire entities, if not, schedule spawning
        var squireQuery = SystemAPI.QueryBuilder().WithAll<SquireTag>().Build();
        if (squireQuery.IsEmpty)
        {
            var squirePrefab = SystemAPI.GetSingleton<Spawner>().SquirePrefab;
            requests.Add(new SpawnRequest
            {
                Prefab = squirePrefab,
                InitialPosition = spawnPoint.Position,
                Radius = 5
            });
        }

        // Check if there are any hero entities, if not, schedule spawning
        var heroQuery = SystemAPI.QueryBuilder().WithAll<HeroTag>().Build();
        if (heroQuery.IsEmpty)
        {
            var heroPrefab = SystemAPI.GetSingleton<Spawner>().HeroPrefab;
            requests.Add(new SpawnRequest
            {
                Prefab = heroPrefab,
                InitialPosition = spawnPoint.Position,
                Radius = 5
            });
        }
        
        // Spawn multiple characters as per the requests
        SpawnMultipleCharacters(ref state, requests, ecb, random);
        
        requests.Dispose();
    }

    private void SpawnMultipleCharacters(ref SystemState state, NativeList<SpawnRequest> spawnRequests,
        EntityCommandBuffer ecb, Random random, int maxAttempts = 50)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var collisionWorld = physicsWorld.PhysicsWorld.CollisionWorld;

        var spawnedPositions = new NativeList<float3>(Allocator.Temp);

        foreach (var request in spawnRequests)
        {
            bool positionFound = false;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Generate a random offset within the spawn radius
                float2 randomOffset = random.NextFloat2Direction() * request.Radius;
                float3 testPosition = request.InitialPosition + new float3(randomOffset.x, 0, randomOffset.y);

                // Check if the position is not occupied
                if (!IsPositionOccupied(collisionWorld, testPosition, spawnedPositions))
                {
                    // Instantiate entity at the valid position
                    Entity instance = ecb.Instantiate(request.Prefab);
                    LocalTransform prefabTransform = SystemAPI.GetComponent<LocalTransform>(request.Prefab);
                    ecb.SetComponent(instance, new LocalTransform
                    {
                        Position = testPosition,
                        Rotation = prefabTransform.Rotation,
                        Scale = prefabTransform.Scale
                    });

                    spawnedPositions.Add(testPosition);
                    positionFound = true;
                    break;
                }
            }

            // If no position was found within the attempts, use the initial position
            if (!positionFound)
            {
                Entity instance = ecb.Instantiate(request.Prefab);
                LocalTransform prefabTransform = SystemAPI.GetComponent<LocalTransform>(request.Prefab);
                ecb.SetComponent(instance, new LocalTransform
                {
                    Position = request.InitialPosition,
                    Rotation = prefabTransform.Rotation,
                    Scale = prefabTransform.Scale
                });
                spawnedPositions.Add(request.InitialPosition);
            }
        }

        spawnedPositions.Dispose();
    }
    
    private bool IsPositionOccupied(CollisionWorld collisionWorld, float3 position, NativeList<float3> spawnedPositions)
    {
        // Check against existing physics bodies
        var filter = CollisionFilter.Default;
        filter.CollidesWith = ~((uint)CollisionLayers.Ground);
        if (PhysicsOverlap(collisionWorld, position, 0.5f, filter))
        {
            return true;
        }

        // Check against other spawn positions
        for (int i = 0; i < spawnedPositions.Length; i++)
        {
            // Assuming a minimum distance of 1 unit between spawns
            if (math.distancesq(position, spawnedPositions[i]) < 1f) 
            {
                return true;
            }
        }

        return false;
    }
    
    private bool PhysicsOverlap(CollisionWorld collisionWorld, float3 position, float radius, CollisionFilter filter)
    {
        var aabb = new Aabb
        {
            Min = position - new float3(radius),
            Max = position + new float3(radius)
        };

        var input = new OverlapAabbInput
        {
            Aabb = aabb,
            Filter = filter
        };

        NativeList<int> allHits = new NativeList<int>(Allocator.Temp);
        bool hasOverlap = collisionWorld.OverlapAabb(input, ref allHits);
        allHits.Dispose();
        return hasOverlap;
    }
}
