using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject SquirePrefab;
    public GameObject HeroPrefab;

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Spawner
            {
                SquirePrefab = GetEntity(authoring.SquirePrefab, TransformUsageFlags.Dynamic),
                HeroPrefab = GetEntity(authoring.HeroPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

struct Spawner : IComponentData
{
    public Entity SquirePrefab;
    public Entity HeroPrefab;
}