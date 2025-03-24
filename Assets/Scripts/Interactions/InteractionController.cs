using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Serialization;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private GameObject planet;
    [SerializeField] private GameObject interactionObjects;
    
    private SphereCollider planetCollider;

    private void Start()
    {
        planetCollider = planet.GetComponent<SphereCollider>();
    }

    /// <summary>
    /// Function that creates interaction objects in random position
    /// </summary>
    /// <param name="numberOfObjects">number of interaction objects</param>
    public void GetRandomPositionForInteractionObjects(int numberOfObjects)
    {
        var goldenRatio = (Mathf.Sqrt(5.0f) + 1.0f) / 2.0f;
        var goldenAngle = (2.0f - goldenRatio) * (2.0f * Mathf.PI);

        var points = new Vector3[numberOfObjects];

        for (int i = 1; i <= numberOfObjects; i++)
        {
            var lat = Mathf.Asin(-1.0f + 2.0f * (i / (float)(numberOfObjects + 1)));
            var lon = goldenAngle * i;

            var x = Mathf.Cos(lon) * Mathf.Cos(lat);
            var y = Mathf.Sin(lon) * Mathf.Cos(lat);
            var z = Mathf.Sin(lat);

            points[i - 1] = new Vector3(x, y, z);
        }

        foreach (var point in points)
        {
            var spawnPosition = planetCollider.transform.position + point * planetCollider.radius * planet.transform.localScale.x;
            Instantiate(interactionObjects, spawnPosition, Quaternion.LookRotation(point));
        }
    }
}