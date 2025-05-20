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
    
    public void SetAnimals(Transform player)
    {
        var v = Random.Range(0, animals.Count);
        
        Instantiate(animals[v], player);
    }
}
