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

        private readonly NetworkList<int> spawnedAnimals = new();

        public int id;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
        }

        private void OnPreShutdown()
        {
            NetworkManager.Singleton.OnPreShutdown -= OnPreShutdown;
            
            RemoveRpc(id);
        }

        protected override void OnNetworkSessionSynchronized()
        {
            base.OnNetworkSessionSynchronized();

            id = GetRandomIndexExcludingSpawned();
            
            SpawnPlayer(id);
        }

        [Rpc(SendTo.Owner)]
        private void AddRpc(int index)
        {
            spawnedAnimals.Add(index);
        }
        
        [Rpc(SendTo.Owner)]
        private void RemoveRpc(int index)
        {
            spawnedAnimals.Remove(index);
        }
        
        private int GetRandomIndexExcludingSpawned()
        {
            var candidates = Enumerable
                .Range(0, animalPrefabs.Count)
                .Where(i => !spawnedAnimals.Contains(i))
                .ToList();

            if (candidates.Count == 0)
            {
                Debug.LogWarning("사용 가능한 인덱스가 남아있지 않습니다!");
                return 0;
            }

            var pick = candidates[Random.Range(0, candidates.Count)];

            return pick;
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
            
            AddRpc(index);
        }
    }
}