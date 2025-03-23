using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private GameObject worldObject;
    [SerializeField] private GameObject interactionObjects;
    public SphereCollider SphereCollider;

    private void Start()
    {
        SphereCollider = worldObject.GetComponent<SphereCollider>();
    }

    /// <summary>
    /// Function that creates interaction objects in random position
    /// </summary>
    /// <param name="numberOfObjects">number of interaction objects</param>
    public void GetRandomPositionForInteractionObjects(int numberOfObjects)
    {
        float goldenRatio = (Mathf.Sqrt(5.0f) + 1.0f) / 2.0f;
        float goldenAngle = (2.0f - goldenRatio) * (2.0f * Mathf.PI);

        Vector3[] points = new Vector3[numberOfObjects];

        for (int i = 1; i <= numberOfObjects; i++)
        {
            float lat = Mathf.Asin(-1.0f + 2.0f * (i / (float)(numberOfObjects + 1)));
            float lon = goldenAngle * i;

            float x = Mathf.Cos(lon) * Mathf.Cos(lat);
            float y = Mathf.Sin(lon) * Mathf.Cos(lat);
            float z = Mathf.Sin(lat);

            points[i - 1] = new Vector3(x, y, z);
        }

        foreach (Vector3 point in points)
        {
            Vector3 spawnPosition = SphereCollider.transform.position + point * SphereCollider.radius * Ints.SCALE_OF_WORLD;
            Instantiate(interactionObjects, spawnPosition, Quaternion.LookRotation(point));
        }
    }
}