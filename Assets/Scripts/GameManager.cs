using System;
using Characters;
using EventHandler;
using Networks;
using UI;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using Utils;

public class GameManager : NetworkBehaviour
{
    public enum GameState
    {
        Title,
        Lobby,
        InGame,
        Result
    }

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

    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        base.OnOwnershipChanged(previous, current);

        MyLogger.Trace($"previous: {previous}\n current: {current}");
    }

    internal void Ready()
    {
        var checker = NetworkManager.Singleton.LocalClient.PlayerObject
            .GetComponent<PlayerReadyChecker>();

        checker.Toggle();
    }

    internal void GameStartRpc()
    {
        try
        {
            if (!ConnectionManager.Instance.CurrentSession.IsHost) return;

            if (!CanGameStart()) throw new Exception("플레이어들이 준비되지 않았습니다");

            GamePlayEventHandler.GameStart();

            LoadSceneRpc("InGame");
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
                .Value = true;
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

    [Rpc(SendTo.Authority)]
    internal void LoadSceneRpc(string sceneName)
    {
        print($"{sceneName} GameStartRpc called");
        print($"client-{NetworkManager.Singleton.CurrentSessionOwner} is Session Owner.");

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}