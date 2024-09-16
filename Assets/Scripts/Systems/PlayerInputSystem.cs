

using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial class PlayerInputSystem : SystemBase
{
    private PhysicsWorldSingleton physicsWorld;
    private SquireInputActions squireInputActions;
    private Vector2 pointerPosition;
    
    protected override void OnCreate()
    {
        RequireForUpdate<PlayerTag>();

        squireInputActions = new SquireInputActions();
        physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
    }
    
    protected override void OnStartRunning()
    {
        squireInputActions.Enable();
        squireInputActions.DungeonMap.Scroll.performed += OnScroll;
    } 

    protected override void OnUpdate()
    {
        pointerPosition = squireInputActions.DungeonMap.PointerPosition.ReadValue<Vector2>();
        
    }

    protected override void OnStopRunning()
    {
        squireInputActions.Disable();
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
}