

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Physics;
using RaycastHit = Unity.Physics.RaycastHit;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial class PlayerInputSystem : SystemBase
{
    private SquireInputActions squireInputActions;
    private Vector2 pointerPosition;
    
    protected override void OnCreate()
    {
        RequireForUpdate<PlayerTag>();

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
        var rayEnd = ray.GetPoint(100f);

        if (Raycast(rayStart, rayEnd, out var raycastHit))
        {
            Debug.Log("Raycast hit!");
        }
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