using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct GameJoinRequest : IRpcCommand
{
    public FixedString32Bytes PlayerId;
}

public class GameJoinRequestAuthoring : MonoBehaviour
{
    public class GameJoinRequestBaker : Baker<GameJoinRequestAuthoring>
    {
        public override void Bake(GameJoinRequestAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<GameJoinRequest>(entity);
        }
    }
}