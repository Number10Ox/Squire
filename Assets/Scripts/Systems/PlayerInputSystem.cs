

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Physics;
using RaycastHit = Unity.Physics.RaycastHit;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    private SquireInputActions squireInputActions;
    private Vector2 pointerPosition;
    
    protected override void OnCreate()
    {
        RequireForUpdate<PlayerTag>();
        RequireForUpdate<DirectoryManaged>();

        squireInputActions = new SquireInputActions();
    }
    
    protected override void OnStartRunning()
    {
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

        // Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 2f); // Draw the ray for 2 se

        if (Raycast(rayStart, rayEnd, out var raycastHit))
        {
            Debug.LogFormat("Raycast hit! Position: {0}, Entity{1}", raycastHit.Position, raycastHit.Entity);

            bool raycastHitFloor = RaycastHitGround(raycastHit);
            if (raycastHitFloor)
            {
                Debug.Log("Enabling TargetPosition on PlayerEntity!");
                
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>(); 
                SystemAPI.SetComponentEnabled<TargetPosition>(playerEntity, true);
                var targetPositionComponent = SystemAPI.GetComponent<TargetPosition>(playerEntity);
                targetPositionComponent.targetPosition = raycastHit.Position;
            }
            else
            {
                Debug.LogFormat("Enabling TargetEntity on PlayerEntity! Entity: {0}", raycastHit.Entity);
                
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>(); 
                SystemAPI.SetComponentEnabled<TargetEntity>(playerEntity, true);
                var targetEntityComponent = SystemAPI.GetComponent<TargetEntity>(playerEntity);
                targetEntityComponent.targetEntity = raycastHit.Entity;
            }
        }
    }

    private bool RaycastHitGround(RaycastHit raycastHit)
    {
        if (raycastHit.Entity == Entity.Null)
            return true;
        
        var collider = EntityManager.GetComponentData<PhysicsCollider>(raycastHit.Entity);
        uint layer = collider.Value.Value.GetCollisionFilter().BelongsTo;
        return layer == (uint)CollisionLayers.Ground;
    }
    
    // TODONOW
    public void RequestSquireMove(float3 targetPosition)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Get the singleton entity that holds the buffer
        var singletonEntity = entityManager.CreateEntityQuery(typeof(MoveRequestBufferElement)).GetSingletonEntity();

        // Get the dynamic buffer from the singleton entity
        var moveRequestBuffer = entityManager.GetBuffer<MoveRequestBufferElement>(singletonEntity);

        // Add the movement request to the buffer
        moveRequestBuffer.Add(new MoveRequestBufferElement
        {
            TargetPosition = targetPosition
        }); 
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
                BelongsTo = (uint) CollisionLayers.Selection,
                CollidesWith = (uint) (CollisionLayers.Ground | CollisionLayers.Interactable)
            }
        };
        
        return physicsWorld.CastRay(raycastInput, out raycastHit);
    }
}