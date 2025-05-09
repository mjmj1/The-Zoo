using Unity.Netcode;
using UnityEngine;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        [SerializeField] private float spawnRadius = 7.5f;
        
        public override void OnNetworkSpawn()
        {
            MoveAllPlayersToRandomSpawn();
        }
        
        private void MoveAllPlayersToRandomSpawn()
        {
            foreach (var (clientId, value) in NetworkManager.Singleton.ConnectedClients)
            {
                if (value.PlayerObject != null)
                {
                    var randomPos = GetRandomPosition();
                    Debug.Log($"Client {clientId}: Position = {randomPos}");
                }
            }
        }
        
        private Vector3 GetRandomPosition()
        {
            return Random.onUnitSphere.normalized * spawnRadius;
        }
    }
}
