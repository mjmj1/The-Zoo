using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AnimalSelector : MonoBehaviour
{
    private NetworkVariable<List<int>> _selectedAnimals = new();
    
    [SerializeField]
    private List<GameObject> animals = new();

    public static AnimalSelector Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public int GetRandomAnimal()
    {
        return Random.Range(0, animals.Count);
    }
    
    [Rpc(SendTo.Everyone)]
    public void SetAnimalRpc(int index, Transform player)
    {
        Instantiate(animals[index], player);
    }
}
