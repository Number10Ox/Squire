using Unity.Entities;

public struct NetworkEntityReference : IComponentData
{
    public Entity Value;
}