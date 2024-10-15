using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace TMG.NFE_Tutorial
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct InitializeLocalChampSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var entity in SystemAPI.QueryBuilder()
                         .WithAll<GhostOwnerIsLocal>()
                         .WithNone<SquireTag>()
                         .WithNone<OwnerSquireTag>()
                         .Build()
                         .ToEntityArray(Allocator.Temp))
            {
                ecb.AddComponent<OwnerSquireTag>(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}