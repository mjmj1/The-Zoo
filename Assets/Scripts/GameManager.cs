using Characters;
using UI.PlayerList;
using Unity.Collections;
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

    [Rpc(SendTo.Everyone)]
    internal void ReadyRpc(FixedString32Bytes playerId, bool isReady)
    {
        PlayerListView.Instance.OnPlayerReady(playerId.Value, isReady);
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

    internal bool CanGameStart()
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