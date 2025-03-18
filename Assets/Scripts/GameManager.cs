using System;
using Networks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private RectTransform titleCanvas;
    [SerializeField] private RectTransform lobbyCanvas;
    
    public ConnectionManager connectionManager;
    
    void Awake()
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

    void Start()
    {
        connectionManager = FindAnyObjectByType<ConnectionManager>();
        
        titleCanvas.gameObject.SetActive(false);
        lobbyCanvas.gameObject.SetActive(false);
    }

    public RectTransform GetTitleCanvas()
    {
        return titleCanvas;
    }
    
    public RectTransform GetLobbyCanvas()
    {
        return lobbyCanvas;
    }
}
