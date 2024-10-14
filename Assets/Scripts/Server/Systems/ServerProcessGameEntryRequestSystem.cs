
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

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
            var playerId = joinRequest.PlayerId; 
            
            // TODONOW
            
            Debug.Log("ServerProcessGameEntryRequestSystem: Connection made");
            ecb.DestroyEntity(requestEntity);
            ecb.AddComponent<NetworkStreamInGame>(requestSource.SourceConnection);
        }

        ecb.Playback(state.EntityManager);
    }
}