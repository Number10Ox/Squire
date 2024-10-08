    
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SquireQuadAuthoring : MonoBehaviour
{
    class Baker : Baker<SquireQuadAuthoring>
    {
        public override void Bake(SquireQuadAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SquireQuadTag());

            if (authoring.transform.parent != null)
            {
                var parentEntity = GetEntity(authoring.transform.parent, TransformUsageFlags.Dynamic);
                AddComponent(entity, new Parent { Value = parentEntity });
            }
        }
    }
}