using Unity.Entities;
using UnityEngine;

public class ExecuteAuthoring : MonoBehaviour
{
    public bool DungeonSync;

    class Baker : Baker<ExecuteAuthoring>
    {
        public override void Bake(ExecuteAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            if (authoring.DungeonSync)
            {
                AddComponent<ExecuteDungeonSync>(entity);
            }
        }
    }
}


public struct ExecuteDungeonSync : IComponentData
{
}