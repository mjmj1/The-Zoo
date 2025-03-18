using Static;
using System;
using System.Collections;
using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    public Transform WorldCenter;
    public float GravityStrength = 9.81f;

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (WorldCenter == null)
        {
            StartCoroutine(GetWorldObject());
        }
        else
        {
            var gravityDirection = (WorldCenter.position - transform.position).normalized;
            var gravity = gravityDirection * GravityStrength;

            _rb.AddForce(gravity, ForceMode.Acceleration);
        }
    }

    IEnumerator GetWorldObject()
    {
        yield return new WaitForSeconds(1);
        WorldCenter = GameObject.FindWithTag(Strings.WORLD).gameObject.GetComponent<Transform>();
    }
}