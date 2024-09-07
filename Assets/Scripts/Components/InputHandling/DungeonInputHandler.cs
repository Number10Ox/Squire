using UnityEngine;
using UnityEngine.InputSystem;

public class DungeonInputHandler : MonoBehaviour
{
    [SerializeField]
    private PlaneCameraController controller;

    public void OnPan(InputValue value)
    {
        Vector2 panInput = value.Get<Vector2>();
        controller.OnPan(panInput);
    }

    public void OnDoubleClick()
    {
        controller.OnDoubleTap();
    }

    public void OnPressBegin()
    {
        controller.OnPressBegin();
    }

    public void OnPressEnd()
    {
        controller.OnPressEnd();
    }

    public void OnScroll(InputValue value)
    {
        Vector2 scrollInput = value.Get<Vector2>();
        controller.OnScroll(scrollInput);
    }

    public void OnPointerPosition(InputValue value)
    {
        Vector2 pointerPosition = value.Get<Vector2>();
        controller.OnPointerPosition(pointerPosition);
    }
}