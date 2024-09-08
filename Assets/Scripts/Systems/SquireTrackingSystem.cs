using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct SquireTrackingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SquireTag>();
        state.RequireForUpdate<DirectoryManaged>();
        state.RequireForUpdate<ExecuteDungeonSync>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Retrieve the DirectoryManaged component
        var directory = SystemAPI.ManagedAPI.GetSingleton<DirectoryManaged>();

        // Query for the entity with SquireTag (there should only be one Squire)
        var squireEntityQuery = SystemAPI.QueryBuilder().WithAll<SquireTag>().Build();

        if (!squireEntityQuery.IsEmpty)
        {
            // Get the LocalTransform (position, rotation, scale) of the Squire entity
            foreach (var localTransform in SystemAPI.Query<LocalTransform>().WithAll<SquireTag>())
            {
                // Update the SquireTrackingTransform's position to match the Squire's position
                directory.SquireTrackingTransform.transform.position = new Vector3(
                    localTransform.Position.x,
                    localTransform.Position.y,
                    localTransform.Position.z
                );
                break; // Only need to update one entity
            }
        }
    }
}