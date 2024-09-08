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