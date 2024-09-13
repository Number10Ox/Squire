using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct SquireRotationJob : IJobEntity
{
    // Ensure the job operates on entities with both LocalTransform and SquireTag
    public void Execute(ref LocalTransform localTransform, in SquireTag squireTag)
    {
        /*
        // Rotate the squire based on its Y position
        if (localTransform.Position.y > 0)
        {
            localTransform.Rotation = quaternion.Euler(90f * Mathf.Deg2Rad, 0f, 0f); // Rotate vertically
        }
        else
        {
            localTransform.Rotation = quaternion.identity; // No rotation (horizontal)
        }
        */
    }
}

public partial class SquireRotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var rotationJob = new SquireRotationJob();
        
        // Schedule the job for entities that have both LocalTransform and SquireTag components
        Dependency = rotationJob.ScheduleParallel(Dependency);
    }
}