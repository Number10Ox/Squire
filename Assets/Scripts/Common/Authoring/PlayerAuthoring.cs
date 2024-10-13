using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public class PlayerAuthoring : MonoBehaviour
{
    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var playerEntity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerTag>(playerEntity);
            AddComponent(playerEntity, new TargetPosition()
            {
                targetPosition = float3.zero,
            });
            AddComponent(playerEntity, new TargetEntity()
            {
                target = Entity.Null
            });
            
            SetComponentEnabled<TargetPosition>(playerEntity, false);
            SetComponentEnabled<TargetEntity>(playerEntity, false);
        }
    }
}