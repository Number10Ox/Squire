using System;
using UnityEngine;

[Serializable]

public struct ZoomBounds
{
    [SerializeField]
    private float nearestZoom;

    [SerializeField]
    private float furthestZoom;

    [SerializeField]
    private float startingZoom;

    [SerializeField]
    private float zoomCooldown;

    [SerializeField]
    private float minXRotation;

    [SerializeField]
    private float maxXRotation;

    [SerializeField]
    private bool useDolly;

    [SerializeField]
    private float rotationUpdateTime;

    private float cooldownTimer;
    public float StartingValue => startingZoom;
    public float MinXRotation => minXRotation;
    public float MaxXRotation => maxXRotation;
    public bool UseDolly => useDolly;

    public float ApplyBounds(float zoomValue)
    {
        return Mathf.Clamp(zoomValue, nearestZoom, furthestZoom);
    }

    public void StartCooldown()
    {
        cooldownTimer = zoomCooldown;
    }

    public void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public bool IsCooldownEnd()
    {
        return cooldownTimer <= 0f;
    }

    public float GetMinZoom()
    {
        return nearestZoom;
    }

    public float GetMaxZoom()
    {
        return furthestZoom;
    }

    public float GetRotationUpdateTime()
    {
        return rotationUpdateTime;
    }
}