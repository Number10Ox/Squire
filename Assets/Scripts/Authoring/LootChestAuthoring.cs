using Unity.Entities;
using UnityEngine;
public class LootChestAuthoring : MonoBehaviour
{
    public class LootChestBaker : Baker<LootChestAuthoring>
    {
        public override void Bake(LootChestAuthoring authoring)
        {
            var playerEntity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<LootChest>(playerEntity);
            AddComponent<Interactable>(playerEntity);
        }
    }
}