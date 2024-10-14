using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerProcessGameEntryRequestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<GameJoinRequest, ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (joinRequest, requestSource, requestEntity) in
                 SystemAPI.Query<GameJoinRequest, ReceiveRpcCommandRequest>().WithEntityAccess())

        {
            Debug.Log("ServerProcessGameEntryRequestSystem: Connection made");

            var playerId = joinRequest.PlayerId;
            SpawnPlayer(ref state, ecb, playerId);

            ecb.DestroyEntity(requestEntity);
            ecb.AddComponent<NetworkStreamInGame>(requestSource.SourceConnection);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void SpawnPlayer(ref SystemState state, EntityCommandBuffer ecb, FixedString32Bytes playerId)
    {
        var spawnRequestBufferEntity = SystemAPI.GetSingletonEntity<SpawnRequestElement>();

        var squireSpawnPointEntity = SystemAPI.GetSingletonEntity<SquireSpawnPointTag>();
        var spawnPoint = SystemAPI.GetComponent<SpawnPoint>(squireSpawnPointEntity);

        var squirePrefab = SystemAPI.GetSingleton<Spawner>().SquirePrefab;
        ecb.AppendToBuffer(spawnRequestBufferEntity, new SpawnRequestElement
        {
            Prefab = squirePrefab,
            InitialPosition = spawnPoint.Position,
            Radius = 5
        });

        var heroPrefab = SystemAPI.GetSingleton<Spawner>().HeroPrefab;
        ecb.AppendToBuffer(spawnRequestBufferEntity, new SpawnRequestElement
        {
            Prefab = heroPrefab,
            InitialPosition = spawnPoint.Position,
            Radius = 5
        });
    }
}