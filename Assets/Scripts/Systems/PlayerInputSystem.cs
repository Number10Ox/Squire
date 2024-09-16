

using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial class PlayerInputSystem : SystemBase
{
    private SquireInputActions squireInputActions;
    
    protected override void OnCreate()
    {
        RequireForUpdate<PlayerTag>();

        squireInputActions = new SquireInputActions();
    }
    
    protected override void OnStartRunning()
    {
        squireInputActions.Enable();
    } 

    protected override void OnUpdate()
    {
        var pointerPosition = squireInputActions.DungeonMap.PointerPosition.ReadValue<Vector2>();
        Debug.LogFormat("pointerPosition - {0}", pointerPosition);
    }

    protected override void OnStopRunning()
    {
        squireInputActions.Disable();
    }
}