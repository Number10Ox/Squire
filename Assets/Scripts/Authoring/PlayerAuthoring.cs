using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var playerEntity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerTag>(playerEntity);
        }
    }
}