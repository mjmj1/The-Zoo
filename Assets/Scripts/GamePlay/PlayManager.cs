using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        public static readonly NetworkVariable<int> CurrentTime = new();
        [SerializeField] private float spawnRadius = 7.5f;
        private readonly WaitForSeconds waitDelay = new(1.0f);

        private bool isGameStarted;

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

        protected override void OnNetworkSessionSynchronized()
        {
            if (!SceneManager.GetActiveScene().name.Equals("InGame")) return;

            OnGameStart();
        }

        private void OnGameStart()
        {
            if (!IsSessionOwner) return;

            isGameStarted = true;

            MoveRandomPositionRpc();

            StartCoroutine(CountTime());
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
            while (isGameStarted)
            {
                yield return waitDelay;

                CurrentTime.Value += 1;
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