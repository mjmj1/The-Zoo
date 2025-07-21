using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCameraBase follow;
    public CinemachineOrbitalFollow orbit;

    public static CameraManager Instance { get; private set; }

    private Vector2 range = new (-180, 180);
    public void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Start()
    {
        Find();

        follow.enabled = false;
        orbit.enabled = false;
    }

    public void Find()
    {
        follow = FindFirstObjectByType<CinemachineVirtualCameraBase>();
        orbit = follow.GetComponent<CinemachineOrbitalFollow>();
        follow.enabled = true;
        orbit.enabled = true;
    }

    public void EnableCamera(bool enable)
    {
        follow.enabled = enable;
    }

    public void SetFollowTarget(Transform target)
    {
        follow.Follow = target;
        follow.LookAt = target;
    }

    public void LookAround()
    {
        orbit.HorizontalAxis.Range = range;
    }

    public void LookMove()
    {
        orbit.HorizontalAxis.Range = Vector2.zero;
    }

    public void SetEulerAngles(float angle)
    {
        orbit.HorizontalAxis.Value = angle;
    }

    public float GetY()
    {
        return orbit.transform.rotation.eulerAngles.y;
    }

    public void LookMoveSmooth(float targetYaw)
    {
        var current = orbit.HorizontalAxis.Value;

        var smooth  = Mathf.MoveTowardsAngle(current, targetYaw, 100 * Time.deltaTime);
        orbit.HorizontalAxis.Value = smooth;
    }
}