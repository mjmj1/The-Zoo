using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Utils;
using TMPro.EditorUtilities;

public class InteractionController : NetworkBehaviour
{
    [SerializeField] private GameObject planet;
    [SerializeField] private GameObject[] interactionObjects;

    private SphereCollider _planetCollider;

    // Tree.cs에서 사용하기 위한 변수
    public NetworkVariable<Vector3>[] SpawnPositon;
    public Vector3 InitialPosition;
    public NetworkVariable<Quaternion>[] SpawnRotation;
    public Quaternion InitialRotation;
    public int NumberOfObjects = 15;
    

    public override void OnNetworkSpawn()
    {
        
        if (!IsSessionOwner) return;

        SpawnPositon = new NetworkVariable<Vector3>[NumberOfObjects];
        SpawnRotation = new NetworkVariable<Quaternion>[NumberOfObjects];
        MyLogger.Print(this, "SpawnPositon.Length : " + SpawnPositon.Length);
        for (int i = 0; i <  SpawnPositon.Length; i++)
        {
            SpawnPositon[i].Value = InitialPosition;
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;

            SpawnRotation[i].Value = InitialRotation;
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;

            if (SpawnPositon[i].Value != InitialPosition)
            {
                Debug.LogWarning($"NetworkVariable was {SpawnPositon} upon being spawned" +
                $" when it should have been {InitialPosition}");
            }
            else
            {
                Debug.Log($"NetworkVariable is {SpawnPositon} when spawned.");
            }
            SpawnPositon[i].OnValueChanged += OnPositionChanged;
        }

    }
    private void Start()
    {
        
        planet = FindFirstObjectByType<PlanetGravity>().gameObject;
        _planetCollider = planet.GetComponent<SphereCollider>();

        GetRandomPositionForInteractionObjects(15);
    }
    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        StartCoroutine(StartChangingNetworkVariable());
    }

    private void OnPositionChanged(Vector3 previous, Vector3 current)
    {
        Debug.Log($"Detected NetworkVariable Change: Previous: {previous} | Current: {current}");
    }

    private IEnumerator StartChangingNetworkVariable()
    {
        var count = 0;
        var updateFrequency = new WaitForSeconds(0.5f);
        while (count < 4)
        {
            for(int i = 0; i < SpawnPositon.Length; i++)
            {
                SpawnPositon[i].Value += SpawnPositon[i].Value;
            }
            
            count++;
            yield return updateFrequency;
        }
        NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
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
        var count = 0;
        MyLogger.Print(this, "SpawnPosition.Length2 : " + SpawnPositon.Length);
        foreach (var point in points)
        {
            var spawnPosition = _planetCollider.transform.position + point * _planetCollider.radius * planet.transform.localScale.x;
            var normal = (spawnPosition - _planetCollider.transform.position).normalized;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

            if(IsSessionOwner)
            {
                SpawnPositon[count].Value = spawnPosition;
                SpawnRotation[count].Value = rotation;
            }
            
            MyLogger.Print(this, "SpawnPositon.Value : " + SpawnPositon[count].Value);
            Instantiate(interactionObjects[Random.Range(0, 3)], SpawnPositon[count].Value, SpawnRotation[count].Value);
            count++;
        }
    }
}