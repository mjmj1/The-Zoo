using System.Collections.Generic;
using UnityEngine;

public class TrainPlanetGravity : MonoBehaviour
{
    public float gravityStrength = 9.81f;
    private readonly HashSet<Rigidbody> affectedBodies = new();

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    public Vector3 GetGravityDirection(Vector3 position)
    {
        return (transform.position - position).normalized;
    }

    private void ApplyGravity()
    {
        foreach (var rb in affectedBodies)
        {
            if (!rb) continue;

            rb.AddForce(GetGravityDirection(rb.position) * gravityStrength, ForceMode.Acceleration);
        }
    }

    public float GetRadius()
    {
        var size = rend.bounds.size;
        return 0.5f * Mathf.Max(size.x, Mathf.Max(size.y, size.z));
    }

    public void Subscribe(Rigidbody rb)
    {
        if (rb)
            affectedBodies.Add(rb);
    }

    public void Unsubscribe(Rigidbody rb)
    {
        if (rb)
            affectedBodies.Remove(rb);
    }
}