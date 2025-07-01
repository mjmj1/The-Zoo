using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private NetworkList<ClientInfo> clientInfoList;

    private struct ClientInfo : INetworkSerializable, IEquatable<ClientInfo>
    {
        public ulong ClientId;
        public int Standby;

        public ClientInfo(ulong clientId, int standby)
        {
            ClientId = clientId;
            Standby = standby;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Standby);
        }

        public bool Equals(ClientInfo other)
        {
            return ClientId == other.ClientId && Standby == other.Standby;
        }

        public override bool Equals(object obj)
        {
            return obj is ClientInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, Standby);
        }

        public override string ToString()
        {
            return $"{ClientId}-{Standby}";
        }
    }

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

    public override void OnNetworkSpawn()
    {
        clientInfoList = new NetworkList<ClientInfo>();

        clientInfoList.OnListChanged += e =>
        {
            print($"[ClientInfoList] {e.Type}: {e.Value}");
        };

        base.OnNetworkSpawn();
    }

    [Rpc(SendTo.Owner)]
    internal void AddRpc(ulong clientId, int standby = 0)
    {
        var info = new ClientInfo(clientId, standby);

        clientInfoList.Add(info);
    }

    [Rpc(SendTo.Owner)]
    internal void RemoveRpc(ulong clientId)
    {
        var info = Find(clientId);

        if (info != null) clientInfoList.Remove(info.Value);
    }

    [Rpc(SendTo.Owner)]
    internal void UpdateStandbyRpc(ulong clientId)
    {
        var index = GetIndex(clientId);

        var info = clientInfoList[index];

        info.Standby = 1;

        clientInfoList[index] = info;
    }

    private ClientInfo? Find(ulong clientId)
    {
        foreach (var info in clientInfoList)
        {
            if (info.ClientId != clientId) continue;

            return info;
        }

        return null;
    }

    public void Print()
    {
        foreach (var info in clientInfoList)
        {
            print(info.ToString());
        }
    }

    private int GetIndex(ulong clientId)
    {
        for (var i = 0; i < clientInfoList.Count; i++)
        {
            if (clientInfoList[i].ClientId != clientId) continue;

            return i;
        }

        return -1;
    }

    internal void LoadLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }

    [Rpc(SendTo.Owner)]
    internal void LoadSceneRpc(string sceneName)
    {
        print($"{sceneName} GameStartRpc called");
        // NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}