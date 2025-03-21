using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;
    [SerializeField] private GameObject worldObject;
    [SerializeField] private GameObject interactionObjects;
    public SphereCollider SphereCollider;

    private void Start()
    {
        SphereCollider = worldObject.GetComponentInChildren<SphereCollider>();

        gameStartButton.onClick.AddListener(OnGameStartButtonClick);
    }
    
    public void OnGameStartButtonClick()
    {
        Vector3[] points = GenerateFibonacciSphere(10);

        foreach(Vector3 point in points)
        {
            //Debug.Log("point : " + point);
            Vector3 spawnPosition = SphereCollider.transform.position + point * SphereCollider.radius * Ints.SCALE_OF_WORLD; // becuase of 'scale of the world', it needs multiplication with actual radius scale
            //Debug.Log("spawnPosition : " + spawnPosition);
            Instantiate(interactionObjects, spawnPosition, Quaternion.LookRotation(point));
        }

        //GameObject objects = Instantiate(interactionObjects, GetRandomPositionOnSphere(SphereCollider.center, SphereCollider.radius), Quaternion.identity);
        //StartCoroutine(PauseObjectsRoutine(objects));
    }
    /// <summary>
    /// Getting random position to spawn objects on the surface of the World map
    /// </summary>
    /// <param name="numPoints">상호작용 오브젝트 개수</param>
    /// <returns>구 형태의 맵 표면</returns>
    public static Vector3[] GenerateFibonacciSphere(int numPoints)
    {
        float goldenRatio = (Mathf.Sqrt(5.0f) + 1.0f) / 2.0f; // 황금 비율 (1.618...)
        float goldenAngle = (2.0f - goldenRatio) * (2.0f * Mathf.PI); // 황금각 (2.399...)

        Vector3[] points = new Vector3[numPoints];

        for (int i = 1; i <= numPoints; i++)
        {
            float lat = Mathf.Asin(-1.0f + 2.0f * (i / (float)(numPoints + 1))); // 위도 계산
            float lon = goldenAngle * i; // 경도 계산

            float x = Mathf.Cos(lon) * Mathf.Cos(lat);
            float y = Mathf.Sin(lon) * Mathf.Cos(lat);
            float z = Mathf.Sin(lat);

            points[i - 1] = new Vector3(x, y, z);
        }

        return points;
    }
}