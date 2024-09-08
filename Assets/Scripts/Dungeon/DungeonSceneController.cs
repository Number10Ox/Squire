using UnityEngine;

public class DungeonSceneController : MonoBehaviour
{
    [SerializeField]
    private PlaneCameraController cameraController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraController.PanImmediately(new Vector3(0, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
