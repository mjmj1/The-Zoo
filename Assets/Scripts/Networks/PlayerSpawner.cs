using System.Collections.Generic;
using System.Linq;
using Static;
using Unity.Netcode;
using UnityEngine;

namespace Networks
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private List<NetworkObject> animalPrefabs;

        private NetworkVariable<int> _nextAnimalIndex = new();
        
        private List<int> _animalIndexes;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _animalIndexes = Enumerable.Range(0, animalPrefabs.Count).OrderBy(_ => Random.value).ToList();
        }

        protected override void OnNetworkSessionSynchronized()
        {
            if (IsSessionOwner)
            {
                _nextAnimalIndex.Value += 1 % _animalIndexes.Count;
            
                var index = _animalIndexes[_nextAnimalIndex.Value];
                
                AssignAnimalPrefab(index);
            }
            else
            {
                AssignAnimalPrefabRpc(NetworkManager.LocalClientId);
            }
            
            base.OnNetworkSessionSynchronized();
        }

        [Rpc(SendTo.Owner)]
        private void AssignAnimalPrefabRpc(ulong clientId)
        {
            print($"Send to owner from client-{clientId}");
            
            _nextAnimalIndex.Value += 1 % _animalIndexes.Count;
            
            var index = _animalIndexes[_nextAnimalIndex.Value];
            
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
            
            prefab.InstantiateAndSpawn(NetworkManager,
                NetworkManager.LocalClientId,
                isPlayerObject: true);
        }
    }
}