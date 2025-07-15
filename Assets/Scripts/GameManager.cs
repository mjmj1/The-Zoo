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

    public void Test()
    {
        var entity = NetworkManager.Singleton.LocalClient.PlayerObject
            .GetComponent<PlayerEntity>();
        
        entity.isReady.Value = !entity.isReady.Value;
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyReadyRpc(FixedString32Bytes playerId, bool isReady)
    {
        // PlayerListView.OnPlayerReady(playerId.Value, isReady);
    }

    internal void GameReady()
    {
        var entity = NetworkManager.Singleton.LocalClient.PlayerObject
            .GetComponent<PlayerEntity>();

        entity.isReady.Value = !entity.isReady.Value;

        NotifyReadyRpc(AuthenticationService.Instance.PlayerId, entity.isReady.Value);
    }
    
    [Rpc(SendTo.Owner)]
    internal void GameStartRpc()
    {
        if (!ConnectionManager.Instance.CurrentSession.IsHost) return;
        
        if (!CanGameStart()) return;

        LoadSceneRpc("InGame");
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

            NotifyReadyRpc(NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEntity>().playerId.Value, 
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