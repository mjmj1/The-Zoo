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
        
        GameObject objects = Instantiate(interactionObjects, GetRandomPositionOnSphere(SphereCollider.center, SphereCollider.radius), Quaternion.identity);
        StartCoroutine(PauseObjectsRoutine(objects));
        //Debug.Log("GenerateFibonacciSphere : " + GenerateFibonacciSphere(objects.transform.position));
    }

    public IEnumerator PauseObjectsRoutine(GameObject objects)
    {
        yield return new WaitForSeconds(5);
        objects.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }


    Vector3 RandomPosition()
    {
        Vector3 originPosition = interactionObjects.transform.position;

        float range_X = SphereCollider.bounds.size.x;
        float range_Y = SphereCollider.bounds.size.y;
        float range_Z = SphereCollider.bounds.size.z;

        range_X = UnityEngine.Random.Range((range_X / 2) * -1, range_X / 2);
        range_Y = UnityEngine.Random.Range((range_Y / 2) * -1, range_Y / 2);
        range_Z = UnityEngine.Random.Range((range_Z / 2) * -1, range_Z / 2);

        Vector3 RandomPostion = new Vector3(range_X, range_Y, range_Z);

        Vector3 respawnPosition = originPosition + RandomPostion;

        return respawnPosition;
    }

    public static Vector3 GetRandomPositionOnSphere(Vector3 center, float radius)
    {
        float theta = UnityEngine.Random.Range(0f, Mathf.PI * 2f);  // 0 ~ 360도 (경도)
        float phi = Mathf.Acos(UnityEngine.Random.Range(-1f, 1f));  // 0 ~ 180도 (위도)

        float x = Mathf.Sin(phi) * Mathf.Cos(theta);
        float y = Mathf.Sin(phi) * Mathf.Sin(theta);
        float z = Mathf.Cos(phi);

        return center + new Vector3(x, y, z) * radius;
    }

    //public static Vector3 GenerateFibonacciSphere(Vector3 center, float radius)
    //{
    //    int index = 10; // number of dots
    //    float goldenRatio = (Mathf.Sqrt(5.0f) + 1.0f) / 2.0f; // 황금 비율 (1.618...)
    //    float goldenAngle = (2.0f - goldenRatio) * (2.0f * Mathf.PI); // 황금각 (2.399...)

    //    float lat = Mathf.Asin(-1.0f + 2.0f * (index / (float)(totalPoints + 1))); // 위도 계산
    //    float lon = goldenAngle * index; // 경도 계산

    //    float x = Mathf.Cos(lon) * Mathf.Cos(lat);
    //    float y = Mathf.Sin(lon) * Mathf.Cos(lat);
    //    float z = Mathf.Sin(lat);

    //    // 중심 좌표(center)를 반영하여 월드 위치 반환
    //    return center + new Vector3(x, y, z) * radius;
    //}
    /*
    const cos = Math.cos, sin = Math.sin, asin = Math.asin, sqrt = Math.sqrt;
    const M_PI = Math.PI;
    function fibonacci_spiral_sphere(num_points)
    {
        const goldenRatio = (sqrt(5.0) + 1.0) / 2.0;                // golden ratio = 1.6180339887498948482
        const goldenAngle = (2.0 - goldenRatio) * (2.0 * M_PI);     // golden angle = 2.39996322972865332
        let points = [];
        let lat, lon;
        let x, y, z;
        for (let i = 1; i <= num_points; ++i)
        {
            lat = asin(-1.0 + 2.0 * (i / (num_points + 1)));
            lon = goldenAngle * i;
            x = cos(lon) * cos(lat);
            y = sin(lon) * cos(lat);
            z = sin(lat);
            points.push(x, y, z);
        }
        return points;
    }
    */
}