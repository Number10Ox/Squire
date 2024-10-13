using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace TMG.NFE_Tutorial
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct ClientRequestGameEntrySystem : ISystem
    {
        private EntityQuery _pendingNetworkIdQuery;

        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<NetworkId>().WithNone<NetworkStreamInGame>();
            _pendingNetworkIdQuery = state.GetEntityQuery(builder);
            state.RequireForUpdate(_pendingNetworkIdQuery);
            state.RequireForUpdate<GameJoinRequest>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var pendingNetworkIds = _pendingNetworkIdQuery.ToEntityArray(Allocator.Temp);

            foreach (var pendingNetworkId in pendingNetworkIds)
            {
                ecb.AddComponent<NetworkStreamInGame>(pendingNetworkId);
                var requestTeamEntity = ecb.CreateEntity();
                ecb.AddComponent(requestTeamEntity, new GameJoinRequest { });
                ecb.AddComponent(requestTeamEntity, new SendRpcCommandRequest { TargetConnection = pendingNetworkId });
            }

            ecb.Playback(state.EntityManager);
        }
    }
}