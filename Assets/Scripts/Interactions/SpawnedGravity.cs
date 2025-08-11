using System.Globalization;
using UnityEngine;

public class SpawnedGravity : MonoBehaviour
{
    private Rigidbody rb;
    public float rotationSpeed = 50f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        InitializeGravity();
    }

    private void Update()
    {
        AlignToSurface();
    }

    private void InitializeGravity()
    {
        rb.useGravity = !PlanetGravity.Instance;

        PlanetGravity.Instance?.Subscribe(rb);
    }

    private void AlignToSurface()
    {
        if (!PlanetGravity.Instance) return;

        var gravityDirection = -PlanetGravity.Instance.GetGravityDirection(transform.position);

        var targetRotation = Quaternion.FromToRotation(
            transform.up, gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
