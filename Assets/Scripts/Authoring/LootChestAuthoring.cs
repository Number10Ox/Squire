using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

public class LootChestAuthoring : MonoBehaviour
{
    public class LootChestBaker : Baker<LootChestAuthoring>
    {
        public override void Bake(LootChestAuthoring authoring)
        {
            var lootChestEntity = GetEntity(TransformUsageFlags.Dynamic);
        
            AddComponent<LootChest>(lootChestEntity);
            AddComponent<Interactable>(lootChestEntity);
        }
    }
}