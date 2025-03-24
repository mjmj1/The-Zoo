using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    private readonly HashSet<Rigidbody> _affectedBodies = new();
    public float gravityStrength = 9.81f;

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        foreach (var rb in _affectedBodies)
        {
            if (!rb) continue;

            var gravityDirection = (transform.position - rb.position).normalized;
            rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);
        }
    }

    public float GetRadius()
    {
        return transform.localScale.x * 0.5f;
    }

    public void Subscribe(Rigidbody rb)
    {
        if (rb != null)
            _affectedBodies.Add(rb);
    }

    public void Unsubscribe(Rigidbody rb)
    {
        if (rb != null)
            _affectedBodies.Remove(rb);
    }
}