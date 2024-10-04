
using Unity.Entities;
using Unity.Mathematics;

public struct TargetEntity : IComponentData, IEnableableComponent
{
    public Entity target;
}