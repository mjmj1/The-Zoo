using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCameraBase follow;
    public CinemachineOrbitalFollow orbit;

    public static CameraManager Instance { get; private set; }

    public void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);

        Find();
        follow.enabled = false;
    }

    public void Find()
    {
        follow = FindFirstObjectByType<CinemachineVirtualCameraBase>();
        orbit = follow.GetComponent<CinemachineOrbitalFollow>();
        follow.enabled = true;
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

    public float GetEulerAnglesY()
    {
        return follow.transform.rotation.y;
    }
}