using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        [SerializeField] private float spawnRadius = 7.5f;
        
        public static readonly NetworkVariable<int> CurrentTime = new();
        
        private bool _isGameStarted;
        private WaitForSeconds _wait;
        
        public void Update()
        {
            if (IsClient)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    PingToAuthorityRpc();    
                }
                else if (Input.GetKeyDown(KeyCode.T))
                {
                    PingToNotAuthorityRpc();
                }
                else if (Input.GetKeyDown(KeyCode.Y))
                {
                    PingToEveryoneRpc();
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!IsSessionOwner) return;

            _isGameStarted = true;
            
            _wait = new WaitForSeconds(1.0f);
            
            MoveRandomPositionRpc();
            
            StartCoroutine(CountTime());
        }

        [Rpc(SendTo.Authority)]
        private void PingToAuthorityRpc()
        {
            print($"Ping SendTo.Authority");
            
            print($"Ping SendTo.Authority from Client {NetworkManager.Singleton.LocalClientId}");
        }
        
        [Rpc(SendTo.NotAuthority)]
        private void PingToNotAuthorityRpc()
        {
            print($"Ping SendTo.NotAuthority");
            
            print($"Ping SendTo.NotAuthority from Client {NetworkManager.Singleton.LocalClientId}");
        }
        
        [Rpc(SendTo.Everyone)]
        private void PingToEveryoneRpc()
        {
            print($"Ping SendTo.Everyone");
            
            print($"Ping SendTo.Everyone from Client {NetworkManager.Singleton.LocalClientId}");
        }

        private IEnumerator CountTime()
        {
            while (_isGameStarted)
            {
                yield return _wait;

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