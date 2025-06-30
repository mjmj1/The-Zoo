using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private GameObject planet;
    [SerializeField] private GameObject[] interactionObjects;

    private SphereCollider _planetCollider;

    // Tree.cs에서 사용하기 위한 변수
    public Vector3 SpawnPositon;
    public Quaternion spawnRotation;

    private void Start()
    {
        _planetCollider = planet.GetComponent<SphereCollider>();

        GetRandomPositionForInteractionObjects(15);
    }

    /// <summary>
    /// A function that creates interaction objects in random position
    /// </summary>
    /// <param name="numberOfObjects">number of interaction objects</param>
    public void GetRandomPositionForInteractionObjects(int numberOfObjects)
    {
        var points = new Vector3[numberOfObjects];

        for (int i = 0; i < numberOfObjects; i++)
        {
            points[i] = Random.onUnitSphere.normalized;
        }

        foreach (var point in points)
        {
            var spawnPosition = _planetCollider.transform.position + point * _planetCollider.radius * planet.transform.localScale.x;
            var normal = (spawnPosition - _planetCollider.transform.position).normalized;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

            SpawnPositon = spawnPosition;
            spawnRotation = rotation;

            Instantiate(interactionObjects[Random.Range(0, 3)], spawnPosition, rotation);
        }
    }
}