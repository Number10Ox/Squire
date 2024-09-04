using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneCameraInputHandler : MonoBehaviour, Dungeon_InputActions.IDungeon_CameraActions
{
    [SerializeField]
    private PlaneCameraController controller;
    
    private Dungeon_InputActions inputActions;

    private void Awake()
    {
        inputActions = new Dungeon_InputActions();
        inputActions.Dungeon_Camera.SetCallbacks(this);
        inputActions.Dungeon_Camera.Enable();
    }

    public void OnPan(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            controller.OnPan(context);
        }
    }
    public void OnDoubleClick(InputAction.CallbackContext context)
    {
        controller.OnDoubleTap(context);
    }

    public void OnPressBegin(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            controller.OnPressBegin(context);
        }
    }

    public void OnPressEnd(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            controller.OnPressEnd(context);
        }
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        controller.OnScroll(context);
    }
}