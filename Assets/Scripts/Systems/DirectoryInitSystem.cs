using System;
using Unity.Entities;
using UnityEngine;
using Unity.Burst;

public partial struct DirectoryInitSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // We need to wait for the scene to load before Updating, so we must RequireForUpdate at
        // least one component type loaded from the scene.
        state.RequireForUpdate<ExecuteDungeonSync>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        var go = GameObject.Find("DungeonGameObjectDirectory");
        if (go == null)
        {
            throw new Exception("GameObject 'DungeonGameObjectDirectory' not found.");
        }

        var directory = go.GetComponent<DungeonGameObjectDirectory>();

        var directoryManaged = new DirectoryManaged();
        directoryManaged.SquireSpawnPoint = directory.SquireSpawnPoint;
        directoryManaged.SquireTrackingTransform = directory.SquireTrackingTransform;
        directoryManaged.DungeonCameraController = directory.DungeonCameraController;

        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(entity, directoryManaged);
    }
}

public class DirectoryManaged : IComponentData
{
    public GameObject SquireSpawnPoint;
    public GameObject SquireTrackingTransform;
    public GameObject DungeonCameraController;

    // Every IComponentData class must have a no-arg constructor.
    public DirectoryManaged()
    {
    }
}