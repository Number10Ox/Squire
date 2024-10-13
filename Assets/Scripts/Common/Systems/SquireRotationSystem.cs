using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct SquireRotationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        /*
        // Query for all entities with the SquireQuadTag
        foreach (var (localTransform, localToWorld) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<LocalToWorld>>().WithAll<SquireQuadTag>())
        {
            // Get world position
            float3 worldPosition = localToWorld.ValueRO.Position;

            // Define raycast parameters using world position
            Vector3 rayStart = worldPosition; // Convert to Unity Vector3
            // Debug.LogFormat("rayStart: {0}", rayStart.ToString());

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit))
            {
                // Calculate the distance from the SquireQuad to the ground
                float distanceToGround = hit.distance;

                //Debug.LogFormat("After Rotation: {0}", localTransform.ValueRW.Rotation.ToString()); 
                
                // If the distance to the ground is greater than 5 units, rotate the SquireQuad entity
                if (distanceToGround > 6f)
                {
                    localTransform.ValueRW.Rotation = quaternion.identity;
                }
                else
                {
                    localTransform.ValueRW.Rotation = quaternion.Euler(math.radians(90f), 0f, 0f); // Rotate vertically
                }


                //Debug.LogFormat("After Rotation: {0}", localTransform.ValueRW.Rotation.ToString()); 

                // Optional: Log the distance for debugging
                // UnityEngine.Debug.Log($"Distance to Ground: {distanceToGround}");
            }
        }
        */
    }
}