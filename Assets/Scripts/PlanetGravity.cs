using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    private static readonly HashSet<Rigidbody> affectedBodies = new();
    public float gravityStrength = 9.81f;

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        foreach (var rb in affectedBodies)
        {
            if (!rb) continue;

            var gravityDirection = (transform.position - rb.position).normalized;
            rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);
        }
    }

    public void Subscribe(Rigidbody rb)
    {
        if (rb != null)
            affectedBodies.Add(rb);
    }

    public void Unsubscribe(Rigidbody rb)
    {
        if (rb != null)
            affectedBodies.Remove(rb);
    }
}