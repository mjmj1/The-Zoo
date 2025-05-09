using Unity.Netcode;
using UnityEngine;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        [SerializeField] private float spawnRadius = 7.5f;
        
        public override void OnNetworkSpawn()
        {
            if (!IsSessionOwner) return;
            
            MoveAllPlayersToRandomSpawn();
        }
        
        private void MoveAllPlayersToRandomSpawn()
        {
            foreach (var (clientId, value) in NetworkManager.Singleton.ConnectedClients)
            {
                if (value.PlayerObject != null)
                {
                    MovePlayerRpc(clientId);
                    // value.PlayerObject.transform.position = randomPos;
                }
            }
        }
        
        [Rpc(SendTo.Server)]
        private void MovePlayerRpc(ulong clientId)
        {
            var randomPos = GetRandomPosition();
            
            var obj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            obj.transform.position = randomPos;
            
            Debug.Log($"Client {clientId}: Position = {randomPos}");
        }
        
        private Vector3 GetRandomPosition()
        {
            return Random.onUnitSphere.normalized * spawnRadius;
        }
    }
}
