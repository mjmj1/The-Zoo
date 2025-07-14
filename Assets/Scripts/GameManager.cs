using System;
using Characters;
using Networks;
using UI.PlayerList;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using Utils;

public class GameManager : NetworkBehaviour
{
    public enum GameState
    {
        Lobby,
        InGame,
        GameOver,
        Finished
    }
    
    public NetworkVariable<GameState> CurrentState = new(GameState.Lobby);
    
    public event Action OnGameOver;
    public event Action OnGameFinished;
    
    public event Action<string, bool> OnGameStart;
    
    public static GameManager Instance { get; private set; }

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
    
    [Rpc(SendTo.Owner)]
    internal void SetGameStateOwnerRpc(GameState state)
    {
        MyLogger.Print(this, $"Set GameState: {state} ");
        CurrentState.Value = state;
    }

    [Rpc(SendTo.Everyone)]
    private void ReadyRpc(FixedString32Bytes playerId, bool isReady)
    {
        // PlayerListView.Instance.OnPlayerReady(playerId.Value, isReady);
    }

    internal void GameReadyRpc()
    {
        var entity = NetworkManager.Singleton.LocalClient.PlayerObject
            .GetComponent<PlayerEntity>();

        entity.isReady.Value = !entity.isReady.Value;

        ReadyRpc(AuthenticationService.Instance.PlayerId, entity.isReady.Value);
    }
    
    [Rpc(SendTo.Owner)]
    internal void GameStartRpc()
    {
        if (!ConnectionManager.Instance.CurrentSession.IsHost) return;
        
        if (!CanGameStart()) return;

        LoadSceneRpc("InGame");
    }

    internal void GameFinishedRpc()
    {
        OnGameFinished?.Invoke();
    }

    internal void GameOverRpc(ulong clientId)
    {
        var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        player.Despawn();
        
        OnGameOver?.Invoke();
    }

    internal void PromotedSessionHost(string playerId)
    {
        if (playerId == AuthenticationService.Instance.PlayerId)
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEntity>().isReady.Value =
                true;
        }
        else
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEntity>().isReady.Value =
                false;

            ReadyRpc(NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEntity>().playerId.Value, 
                false);
        }
    }

    private bool CanGameStart()
    {
        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            if (client.PlayerObject == null) return false;

            if (!client.PlayerObject.TryGetComponent<PlayerEntity>(out var entity)) return false;

            if (!entity.isReady.Value) return false;
        }

        return true;
    }

    internal void LoadLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }

    [Rpc(SendTo.Owner)]
    internal void LoadSceneRpc(string sceneName)
    {
        print($"{sceneName} GameStartRpc called");
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}