using System;
using Characters;
using Networks;
using UI;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

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

    internal void GameStartRpc()
    {
        try
        {
            print(1);
            if (!ConnectionManager.Instance.CurrentSession.IsHost) return;
            print(2);
            if (!CanGameStart()) throw new Exception("You Can not start game !");
            print(3);
            LoadSceneServerRpc("InGame");
            print(4);
        }
        catch (Exception e)
        {
            InformationPopup.instance.ShowPopup(e.Message);
        }
    }

    internal void PromotedSessionHost(string playerId)
    {
        if (playerId == AuthenticationService.Instance.PlayerId)
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerReadyChecker>().isReady
                    .Value =
                true;
        else
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerReadyChecker>().Reset();
    }

    private bool CanGameStart()
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

    internal void LoadLobbyScene()
    {
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void LoadSceneServerRpc(string sceneName)
    {
        print($"{sceneName} GameStartRpc called");
        print($"client-{NetworkManager.Singleton.CurrentSessionOwner} is Session Owner.");

        // NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}