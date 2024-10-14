using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnSystem : ISystem
{
    private const int MAX_SPAWN_ATTEMPTS = 50; 
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Require necessary components for update
        state.RequireForUpdate<Spawner>();
        state.RequireForUpdate<ExecuteDungeonSync>();
        state.RequireForUpdate<SquireSpawnPointTag>();

        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddBuffer<SpawnRequestElement>(entity);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var spawnRequestBuffer = SystemAPI.GetSingletonBuffer<SpawnRequestElement>();
        var spawnRequestBufferEntity = SystemAPI.GetSingletonEntity<SpawnRequestElement>();

        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var collisionWorld = physicsWorld.PhysicsWorld.CollisionWorld;

        var spawnedPositions = new NativeList<float3>(Allocator.Temp);
        
        var random = new Random((uint)System.DateTime.Now.Ticks);

        for (int i = 0; i < spawnRequestBuffer.Length; ++i)
        {
            SpawnRequestElement request = spawnRequestBuffer[i];

            bool positionFound = false;
            for (int attempt = 0; attempt < MAX_SPAWN_ATTEMPTS; attempt++)
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
                    ecb.AddComponent(instance, new GhostOwner { NetworkId = request.OwnerId });

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
        
        // Clear spawn request buffer
        ecb.SetBuffer<SpawnRequestElement>(spawnRequestBufferEntity);
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