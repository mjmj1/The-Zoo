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

        public int index;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
        }

        private void OnPreShutdown()
        {
            NetworkManager.Singleton.OnPreShutdown -= OnPreShutdown;

            RemoveRpc(index);
        }

        protected override void OnNetworkSessionSynchronized()
        {
            Spawn();
        }

        private void Spawn()
        {
            index = GetRandomIndexExcludingSpawned(animalPrefabs.Count);

            SpawnPlayer(index);

            AddRpc(index);

            base.OnNetworkSessionSynchronized();
        }

        [Rpc(SendTo.Owner)]
        private void AddRpc(int i)
        {
            spawnedAnimals.Add(i);
        }

        [Rpc(SendTo.Owner)]
        private void RemoveRpc(int i)
        {
            spawnedAnimals.Remove(i);
        }

        private int GetRandomIndexExcludingSpawned(int max)
        {
            var candidates = Enumerable
                .Range(0, max)
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

        private void SpawnPlayer(int i)
        {
            var prefab = animalPrefabs[i];

            var pos = Util.GetCirclePositions(Vector3.zero, i, 5f, 8);

            prefab.InstantiateAndSpawn(NetworkManager,
                NetworkManager.LocalClientId,
                isPlayerObject: true,
                position: pos,
                rotation: Quaternion.LookRotation((Vector3.zero - pos).normalized));
        }
    }
}