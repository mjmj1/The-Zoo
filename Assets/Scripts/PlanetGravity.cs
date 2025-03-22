using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlanetGravity : MonoBehaviour
{
    public float gravityStrength = 9.81f;
    static HashSet<Rigidbody> affectedBodies = new();

    void FixedUpdate()
    {
        ApplyGravity();
    }
    
    void ApplyGravity()
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
