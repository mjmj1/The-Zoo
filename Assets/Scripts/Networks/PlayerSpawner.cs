using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Networks
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private List<NetworkObject> animalPrefabs;

        private readonly NetworkVariable<int> nextAnimalIndex = new();
        
        private List<int> animalIndexes;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            animalIndexes = Enumerable.Range(0, animalPrefabs.Count).OrderBy(_ => Random.value).ToList();
        }

        protected override void OnNetworkSessionSynchronized()
        {
            if (IsSessionOwner)
            {
                nextAnimalIndex.Value += 1 % animalIndexes.Count;
            
                var index = animalIndexes[nextAnimalIndex.Value];
                
                AssignAnimalPrefab(index);
            }
            else
            {
                AssignAnimalPrefabRpc(NetworkManager.LocalClientId);
            }
            
            base.OnNetworkSessionSynchronized();
        }

        private Vector3 GetCirclePositions(Vector3 center, int index, float radius, int count)
        {
            var angle = index * Mathf.PI * 2f / count;
            var x = center.x + radius * Mathf.Cos(angle);
            var z = center.z + radius * Mathf.Sin(angle);

            return new Vector3(x, center.y, z);
        }

        [Rpc(SendTo.Owner)]
        private void AssignAnimalPrefabRpc(ulong clientId)
        {
            print($"Send to owner from client-{clientId}");
            
            nextAnimalIndex.Value += 1 % animalIndexes.Count;
            
            var index = animalIndexes[nextAnimalIndex.Value];
            
            AssignAnimalPrefabRpc(index, new RpcParams
            {
                Send = RpcTarget.Single(clientId, RpcTargetUse.Temp)
            });
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void AssignAnimalPrefabRpc(int index, RpcParams rpcParams = default)
        {
            print($"Received index: {index}");

            AssignAnimalPrefab(index);
        }

        private void AssignAnimalPrefab(int index)
        {
            var prefab = animalPrefabs[index];

            var pos = GetCirclePositions(Vector3.zero, index, 5f, 8);

            prefab.InstantiateAndSpawn(NetworkManager,
                NetworkManager.LocalClientId,
                isPlayerObject: true,
                position: pos);
        }
    }
}