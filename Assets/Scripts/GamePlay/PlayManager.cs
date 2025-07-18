using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        public enum Team
        {
            Hider,
            Seeker
        }

        public static PlayManager Instance;

        public NetworkVariable<int> currentTime = new();
        [SerializeField] private float spawnRadius = 7.5f;
        private readonly WaitForSeconds waitDelay = new(1.0f);

        private NetworkList<ulong> hiderIds = new();
        private NetworkList<ulong> seekerIds = new();

        public void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void Update()
        {
            if (IsClient)
            {
                if (Input.GetKeyDown(KeyCode.T))
                    PingToAuthorityRpc();
                else if (Input.GetKeyDown(KeyCode.G))
                    PingToNotAuthorityRpc();
                else if (Input.GetKeyDown(KeyCode.B))
                    PingToEveryoneRpc();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            MyLogger.Print(this, "OnNetworkSpawn");

            OnGameStart();
        }

        private void OnGameStart()
        {
            if (!IsSessionOwner) return;

            MyLogger.Print(this, "Game Start");

            MoveRandomPositionRpc();

            AssignRole();

            StartCoroutine(CountTime());
        }

        private void AssignRole()
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            var seeker = clients[Random.Range(0, clients.Count)];

            seekerIds.Add(seeker.ClientId);

            foreach (var client in clients)
            {
                if (seekerIds.Contains(client.ClientId)) return;

                hiderIds.Add(client.ClientId);
            }
        }

        [Rpc(SendTo.Authority)]
        private void PingToAuthorityRpc()
        {
            print("Ping SendTo.Authority");

            print($"Ping SendTo.Authority from Client-{OwnerClientId}");
        }

        [Rpc(SendTo.NotAuthority)]
        private void PingToNotAuthorityRpc()
        {
            print("Ping SendTo.NotAuthority");

            print($"Ping SendTo.NotAuthority from Client-{OwnerClientId}");
        }

        [Rpc(SendTo.Everyone)]
        private void PingToEveryoneRpc()
        {
            print("Ping SendTo.Everyone");

            print($"Ping SendTo.Everyone from Client-{OwnerClientId}");
        }

        private IEnumerator CountTime()
        {
            while (true)
            {
                yield return waitDelay;

                currentTime.Value += 1;
            }
        }

        [Rpc(SendTo.Everyone)]
        private void MoveRandomPositionRpc()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            var randomPos = Util.GetRandomPositionInSphere(spawnRadius);

            var obj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            obj.transform.position = randomPos;

            print($"Client {clientId}: Position = {randomPos}");
        }
    }
}