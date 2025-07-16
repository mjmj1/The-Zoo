using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

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
                
                SpawnPlayer(index);
            }
            else
            {
                SpawnPlayerRpc(NetworkManager.LocalClientId);
            }
            
            base.OnNetworkSessionSynchronized();
        }

        [Rpc(SendTo.Owner)]
        private void SpawnPlayerRpc(ulong clientId)
        {
            nextAnimalIndex.Value += 1 % animalIndexes.Count;
            
            var index = animalIndexes[nextAnimalIndex.Value];
            
            SpawnPlayerRpc(index, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SpawnPlayerRpc(int index, RpcParams rpcParams = default)
        {
            SpawnPlayer(index);
        }

        private void SpawnPlayer(int index)
        {
            var prefab = animalPrefabs[index];

            var pos = Util.GetCirclePositions(Vector3.zero, index, 5f, 8);

            prefab.InstantiateAndSpawn(NetworkManager,
                NetworkManager.LocalClientId,
                isPlayerObject: true,
                position: pos,
                rotation: Quaternion.LookRotation((Vector3.zero - pos).normalized));
        }
    }
}