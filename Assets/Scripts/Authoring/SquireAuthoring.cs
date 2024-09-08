using Unity.Entities;
using UnityEngine;

public class SquireAuthoring : MonoBehaviour
{
    class Baker : Baker<SquireAuthoring>
    {
        public override void Bake(SquireAuthoring authoring)
        {
            // The entity will be moved
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<SquireTag>(entity);
        }
    }
}