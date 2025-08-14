using System;
using Interactions;
using Networks;
using Players;
using UI;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<int> readyCount = new();
    internal PlayerSpawner playerSpawner;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        playerSpawner = GetComponent<PlayerSpawner>();
    }

    [Rpc(SendTo.Authority)]
    internal void ReadyRpc(bool isReady)
    {
        readyCount.Value = isReady ? readyCount.Value + 1 : readyCount.Value - 1;
    }

    internal void Ready()
    {
        var checker = NetworkManager.Singleton.LocalClient.PlayerObject
            .GetComponent<PlayerReadyChecker>();

        ReadyRpc(checker.Toggle());
    }

    internal void GameStartRpc()
    {
        try
        {
            if (!ConnectionManager.Instance.CurrentSession.IsHost) return;

            if (!CanGameStart()) throw new Exception("플레이어들이 준비되지 않았습니다");

            LoadSceneRpc("InGame");

            readyCount.Value = 0;
        }
        catch (Exception e)
        {
            InformationPopup.instance.ShowPopup(e.Message);
        }
    }

    internal void GameEndRpc()
    {
        print("Game EndRpc called");

        NpcSpawner.Instance.DespawnNpcRpc();

        LoadSceneRpc("Lobby");
    }

    internal void PromotedSessionHost(string playerId)
    {
        if (playerId == AuthenticationService.Instance.PlayerId)
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerReadyChecker>().isReady
                .Value = true;
        else
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerReadyChecker>().Reset();
    }

    internal bool CanGameStart()
    {
        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            if (client.PlayerObject == null) return false;

            if (!client.PlayerObject.TryGetComponent<PlayerReadyChecker>(out var checker))
                return false;

            if (!checker.isReady.Value) return false;
        }

        return true;
    }

    [Rpc(SendTo.Authority)]
    private void LoadSceneRpc(string sceneName)
    {
        print($"{sceneName} GameStartRpc called");

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}