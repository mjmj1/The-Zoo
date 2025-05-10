using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        [SerializeField] private float spawnRadius = 7.5f;

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

            MoveAllPlayersToRandomSpawn();
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

        private void MoveAllPlayersToRandomSpawn()
        {
            MoveRandomPositionRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void MoveRandomPositionRpc()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            var randomPos = GetRandomPosition();

            var obj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            obj.transform.position = randomPos;

            print($"Client {clientId}: Position = {randomPos}");
        }

        private Vector3 GetRandomPosition()
        {
            return Random.onUnitSphere.normalized * spawnRadius;
        }
    }
}