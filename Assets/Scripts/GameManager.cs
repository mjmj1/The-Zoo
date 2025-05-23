using Networks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ConnectionManager connectionManager;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        connectionManager = FindAnyObjectByType<ConnectionManager>();
    }
}