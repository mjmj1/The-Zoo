using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] internal CinemachineOrbitalFollow orbit;

    private readonly Vector2 range = new(-180, 180);

    public static CameraManager Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        DontDestroyOnLoad(orbit.gameObject);

        EnableCamera(false);
    }

    public void EnableCamera(bool enable)
    {
        orbit.gameObject.SetActive(enable);
    }

    public void SetFollowTarget(Transform target)
    {
        orbit.VirtualCamera.Follow = target;
        orbit.VirtualCamera.LookAt = target;
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

        var smooth = Mathf.MoveTowardsAngle(current, targetYaw, 100 * Time.deltaTime);
        orbit.HorizontalAxis.Value = smooth;
    }
}