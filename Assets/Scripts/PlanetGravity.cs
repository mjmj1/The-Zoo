using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    public float gravityStrength = 9.81f;
    private readonly HashSet<Rigidbody> affectedBodies = new();

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

    public float GetRadius()
    {
        return transform.localScale.x * 0.5f;
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