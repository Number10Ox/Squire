using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(CinemachineBrain))]
public class PlaneCameraController : MonoBehaviour
{
    public event Action<float> OnZoomChanged;
    public event Action<Vector3, Vector3> OnViewportMoved;

    [SerializeField]
    private Transform cameraLookAt;

    [SerializeField]
    private CinemachineCamera userControlledCamera;

    [SerializeField]
    private float groundPlaneY;

    [SerializeField]
    private bool resetOnDoubleTap;

    [SerializeField]
    private float cameraMoveEpsilon = 0.01f;

    [SerializeField]
    private CameraBounds horizontalBounds;

    [SerializeField]
    private Transform TrackTransform;

    public Camera MainCamera { get; private set; }

    private CinemachinePositionComposer positionComposer;
    private float current;
    private Vector3 startingPosition;
    private Plane groundPlane;
    private Vector3 lastViewportUpdatePosition;
    private int currentTouchCount;
    private bool useDolly;
    private float currentZoom;
    private LTDescr zoomTween;

    private Vector2 pointerPosition;
    private bool pressing;

    [SerializeField]
    private ZoomBounds zoom;

    #region Unity Methods

    void Awake()
    {
        MainCamera = Camera.main;
        startingPosition = CameraLookAtPosition;
        positionComposer = userControlledCamera.GetComponent<CinemachinePositionComposer>();

        if (positionComposer != null)
        {
            float startingZoom = zoom.StartingValue;
            positionComposer.CameraDistance = startingZoom;
            OnZoomChanged?.Invoke(startingZoom);
            currentZoom = startingZoom;
        }
        else
        {
            Debug.LogError("User controlled camera must have configured body position transposer!");
        }

        groundPlane = new Plane(Vector3.up, new Vector3(0.0f, groundPlaneY, 0.0f));
    }

    void OnValidate()
    {
        Vector3 lookAtPosition = cameraLookAt.position;
        lookAtPosition.y = groundPlaneY;
        cameraLookAt.position = lookAtPosition;
    }

    void LateUpdate()
    {
        if (TrackTransform != null)
        {
            CameraLookAtPosition = TrackTransform.position;
        }

        zoom.UpdateCooldown();

        if (ViewportMoved())
        {
            Vector3 currentViewportPosition = MainCamera.transform.position;
            OnViewportMoved?.Invoke(lastViewportUpdatePosition, currentViewportPosition);
            lastViewportUpdatePosition = currentViewportPosition;
        }
    }

    #endregion

    #region Interface for Game-specific InputHandler

    public void OnPan(Vector2 panValue)
    {
        // Handle panning logic
        if (pressing)
        {
            Vector3 startPoint = ScreenPointToGround(pointerPosition - panValue);
            Vector3 endPoint = ScreenPointToGround(pointerPosition);
            Vector3 panAmount = startPoint - endPoint;

            // Debug.LogFormat("OnPan: delta: ({0}, {1}) pointerPos: ({2}, {3}, panAmount: {4}", delta.x, delta.y,
            //    pointerPosition.x,
            //    pointerPosition.y, panAmount);

            PanTo(CameraLookAtPosition + panAmount);
        }
    }

    public void OnScroll(Vector2 scrollValue)
    {
        float delta = scrollValue.y;
        SetZoom(currentZoom - delta);
    }

    public void OnPressBegin()
    {
        pressing = true;
    }

    public void OnPressEnd()
    {
        pressing = false;
    }

    public void OnDoubleTap()
    {
        if (resetOnDoubleTap)
        {
            PanTo(startingPosition, zoom.StartingValue);
        }
    }

    public void OnPointerPosition(Vector2 pointerPosition)
    {
        this.pointerPosition = pointerPosition;
    }

    #endregion

    #region Camera Control Methods

    public void PanTo(Vector3 position)
    {
        PanTo(position, currentZoom);
    }

    public void PanTo(Vector3 position, float zoomAmount)
    {
        CameraLookAtPosition = horizontalBounds.ApplyBounds(position);
        if (zoomAmount != currentZoom)
        {
            SetZoom(zoomAmount);
        }
    }

    public void PanImmediately(Vector3 position)
    {
        userControlledCamera.enabled = false;
        Vector3 delta = position - CameraLookAtPosition;
        CameraLookAtPosition = position;
        userControlledCamera.OnTargetObjectWarped(cameraLookAt, delta);
        userControlledCamera.enabled = true;
    }

    public void FocusAtPosition(Vector3 groundFocusPos, Vector2 cameraSpaceOffset, float zoom)
    {
        //apply camera space offset
        Vector3 cameraAtFlat = userControlledCamera.transform.forward;
        cameraAtFlat.y = 0.0f;
        cameraAtFlat.Normalize();
        Vector3 cameraRightFlat = Vector3.zero;
        cameraRightFlat.x = cameraAtFlat.z;
        cameraRightFlat.z = -cameraAtFlat.x;

        //Z offset
        cameraAtFlat *= cameraSpaceOffset.y;
        groundFocusPos += cameraAtFlat;

        //X offset
        cameraRightFlat *= cameraSpaceOffset.x;
        groundFocusPos += cameraRightFlat;

        //set new target position
        PanTo(groundFocusPos, zoom);
    }

    public virtual Vector3 ViewportToGround()
    {
        return ScreenPointToGround(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
    }

    public Vector3 ScreenPointToGround(Vector2 screenPoint)
    {
        Ray ray = MainCamera.ScreenPointToRay(screenPoint);
        Vector3 planePoint = Vector3.zero;
        float distance;

        if (groundPlane.Raycast(ray, out distance))
        {
            planePoint = ray.GetPoint(distance);
        }

        return planePoint;
    }

    public void SetZoom(float newZoom)
    {
        //force stop camera panning
        CameraLookAtPosition = ViewportToGround();

        if (currentZoom != newZoom)
        {
            currentZoom = zoom.ApplyBounds(newZoom);
            zoom.StartCooldown();
            positionComposer.CameraDistance = currentZoom;

            if (zoom.UseDolly)
            {
                // normalize the current zoom amount from 0 to 1
                float normalizedZoom = (newZoom - zoom.GetMinZoom()) / (zoom.GetMaxZoom() - zoom.GetMinZoom());

                // cancel existing tweens
                if (zoomTween != null)
                {
                    LeanTween.cancel(zoomTween.id);
                }

                // determine a target X rotation, based on a plotted point in an LT graph (liner, easeIn, etc.)
                float targetXRotation = LeanTween.linear(zoom.MinXRotation, zoom.MaxXRotation, normalizedZoom);

                // transition our X rotation of the virtual camera over time
                zoomTween = LeanTween
                    .rotateX(userControlledCamera.gameObject, targetXRotation, zoom.GetRotationUpdateTime())
                    .setOnComplete(() =>
                    {
                        // INFO: FIX: If you manage the tweens state, always set the reference to null in onComplete.
                        // If you don't, you'll hold on to the id and cancel someone elses tween by accident
                        zoomTween = null;
                    });
            }

            OnZoomChanged?.Invoke(currentZoom);
        }
    }

    public Vector3 CameraLookAtPosition
    {
        get => cameraLookAt.position;
        private set => cameraLookAt.position = new Vector3(value.x, groundPlaneY, value.z);
    }

    #endregion

    #region Private Methods

    private bool ViewportMoved()
    {
        Vector3 camPos = MainCamera.transform.position;
        float magnitudeSqr = (camPos - lastViewportUpdatePosition).sqrMagnitude;
        return cameraMoveEpsilon * cameraMoveEpsilon < magnitudeSqr;
    }

    #endregion
}