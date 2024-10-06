using Unity.Entities;
using UnityEngine;
public class HeroAuthoring : MonoBehaviour
{
    public class HeroBaker : Baker<HeroAuthoring>
    {
        public override void Bake(HeroAuthoring authoring)
        {
            var heroEntity = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}