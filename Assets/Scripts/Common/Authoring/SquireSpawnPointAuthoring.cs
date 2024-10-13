using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SquireSpawnPointAuthoring : MonoBehaviour
{
    public Transform position;
    
    class Baker : Baker<SquireSpawnPointAuthoring>
    {
        public override void Bake(SquireSpawnPointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.WorldSpace);
            AddComponent(entity, new SquireSpawnPointTag());
            AddComponent(entity, new SpawnPoint()
            {
               Position = authoring.position.position
            });
        }
    }
}