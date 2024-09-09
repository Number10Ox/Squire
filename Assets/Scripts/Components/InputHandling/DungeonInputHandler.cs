using UnityEngine;
using UnityEngine.InputSystem;

public class DungeonInputHandler : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    public LayerMask groundLayerMask;
    [SerializeField]
    private PlaneCameraController controller;
    
    private Vector2 pointerPosition;
    
    public void OnScroll(InputValue value)
    {
        Vector2 scrollInput = value.Get<Vector2>();
        controller.OnScroll(scrollInput);
    }
    
    public GameObject markerPrefab;  // Assign a sphere or cube prefab in the inspector

    void PlaceMarker(Vector3 position)
    {
        GameObject marker = Instantiate(markerPrefab, position, Quaternion.identity);
        Destroy(marker, 6.0f);
    }

    public void OnAttack()
    {
        // Vector3 groundClickPosition = ScreenPointToGround(pointerPosition);
        // PlaceMarker(groundClickPosition);
        // Debug.LogFormat("pointerPosition: {0}, {1}", pointerPosition.x, pointerPosition.y);
        // Debug.LogFormat("Ground clickPosition: {0}, {1}, {2}", groundClickPosition.x, groundClickPosition.y, groundClickPosition.z);
        
        Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
        // Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 2f); // Draw the ray for 2 se
        
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundLayerMask))
        //if (Physic.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
        {
            // If the ray hits something on the ground layer, teleport the Squire
            Vector3 clickPosition = hitInfo.point;
            Debug.LogFormat("HIT! Raycast clickPosition: {0}, {1}, {2}", clickPosition.x, clickPosition.y, clickPosition.z);
            // TeleportSquire(clickPosition);
        }
        else
        {
            Debug.Log("No raycast hit."); 
        }
    }

    public Vector3 ScreenPointToGround(Vector2 screenPoint)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPoint);
        Vector3 planePoint = Vector3.zero;
        float distance;

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0.0f, 0.0f, 0.0f));
        if (groundPlane.Raycast(ray, out distance))
        {
            planePoint = ray.GetPoint(distance);
        }

        return planePoint;
    }
    
    public void OnPointerPosition(InputValue value)
    {
        pointerPosition = value.Get<Vector2>();
    }
}