using System;
using UnityEngine;

[Serializable]
public class CameraBounds
{
    [SerializeField]
    private bool useBounds;

    [SerializeField]
    [Tooltip("Applicable to horizontal (XZ) plane (X -> X, Y -> Z)")]
    private Vector2 min;

    [SerializeField]
    [Tooltip("Applicable to horizontal (XZ) plane (X -> X, Y -> Z)")]
    private Vector2 max;

    public Vector2 Max
    {
        get => max;
        set => max = value;
    }

    public Vector2 Min
    {
        get => min;
        set => min = value;
    }

    public bool UseBounds
    {
        get => useBounds;
        set => useBounds = value;
    }

    public Vector3 ApplyBounds(Vector3 cameraPosition)
    {
        if (useBounds)
        {
            // Debug.LogFormat("Input bounds x: {0}, y: {1}, ", cameraPosition.x, cameraPosition.y);
            cameraPosition.x = Mathf.Clamp(cameraPosition.x, min.x, max.x);
            cameraPosition.z = Mathf.Clamp(cameraPosition.z, min.y, max.y);
            // Debug.LogFormat("Output bounds x: {0}, y: {1}, ", cameraPosition.x, cameraPosition.y);
        }

        return cameraPosition;
    }
}