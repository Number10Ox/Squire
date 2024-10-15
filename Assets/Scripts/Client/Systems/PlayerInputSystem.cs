using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Physics;
using RaycastHit = Unity.Physics.RaycastHit;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    private SquireInputActions squireInputActions;
    private Vector2 pointerPosition;

    protected override void OnCreate()
    {
        RequireForUpdate<SquireTag>();
        RequireForUpdate<DirectoryManaged>();

        squireInputActions = new SquireInputActions();
    }

    protected override void OnStartRunning()
    {
        Debug.Log("******* Starting running....");
        squireInputActions.Enable();
        squireInputActions.DungeonMap.Scroll.performed += OnScroll;
        squireInputActions.DungeonMap.Interact.performed += OnInteract;
    }

    protected override void OnUpdate()
    {
        pointerPosition = squireInputActions.DungeonMap.PointerPosition.ReadValue<Vector2>();
    }

    protected override void OnStopRunning()
    {
        squireInputActions.Disable();

        squireInputActions.DungeonMap.Scroll.performed -= OnScroll;
        squireInputActions.DungeonMap.Interact.performed -= OnInteract;
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        // TODO handle this directly here?
        // TODO cache these values?
        var directory = SystemAPI.ManagedAPI.GetSingleton<DirectoryManaged>();
        var cameraController = directory.DungeonCameraController;

        Vector2 scrollInput = context.ReadValue<Vector2>();
        cameraController.GetComponent<PlaneCameraController>().OnScroll(scrollInput);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        var directory = SystemAPI.ManagedAPI.GetSingleton<DirectoryManaged>();
        var cameraController = directory.DungeonCameraController;
        var ray = cameraController.GetComponent<Camera>().ScreenPointToRay(pointerPosition);
        var rayStart = ray.origin;
        var rayEnd = ray.GetPoint(1000f);

        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 2f); 
        bool hit = Raycast(rayStart, rayEnd, out var raycastHit);
        if (!hit || raycastHit.Entity == Entity.Null)
            return;
        
        // Debug.LogFormat("Raycast hit! Position: {0}, Entity{1}", raycastHit.Position, raycastHit.Entity);

        CollisionLayers layers = CollisionLayersFromRaycastHit(raycastHit);
        if ((layers & CollisionLayers.Ground) != 0)
        {
            Debug.Log("Enabling *TargetPosition* on PlayerEntity!");

            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var targetPositionComponent = SystemAPI.GetComponent<TargetPosition>(playerEntity);
            targetPositionComponent.Position = raycastHit.Position;

            SystemAPI.SetComponent(playerEntity, targetPositionComponent);
            SystemAPI.SetComponentEnabled<TargetPosition>(playerEntity, true);

            // Debug.LogFormat("Setting targetPositionComponent {0}", targetPositionComponent.TargetPosition);
        }
        else if ((layers & CollisionLayers.Interactable) != 0)
        {
            Debug.Log("Enabling *TargetEntity* on PlayerEntity!");
            
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>(); 
            var targetEntityComponent = SystemAPI.GetComponent<TargetEntity>(playerEntity);
            targetEntityComponent.Target = raycastHit.Entity;
            SystemAPI.SetComponent(playerEntity, targetEntityComponent);
            SystemAPI.SetComponentEnabled<TargetEntity>(playerEntity, true);
        }
    }

    private CollisionLayers CollisionLayersFromRaycastHit(RaycastHit raycastHit)
    {
        var collider = EntityManager.GetComponentData<PhysicsCollider>(raycastHit.Entity);
        uint layers = collider.Value.Value.GetCollisionFilter().BelongsTo;
        return (CollisionLayers)layers;
    }

    private bool Raycast(float3 rayStart, float3 rayEnd, out RaycastHit raycastHit)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var raycastInput = new RaycastInput
        {
            Start = rayStart,
            End = rayEnd,
            Filter = new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayers.Selection,
                CollidesWith = (uint)(CollisionLayers.Ground | CollisionLayers.Interactable)
            }
        };

        return physicsWorld.CastRay(raycastInput, out raycastHit);
    }
}