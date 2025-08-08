using System.Collections.Generic;
using System.Linq;
using EventHandler;
using Unity.Netcode;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Networks
{
    public class PlayerSpawner : NetworkBehaviour
    {
        public static PlayerSpawner Instance { get; private set; }

        [SerializeField] private List<NetworkObject> animalPrefabs;
        private readonly NetworkList<int> spawnedAnimals = new();

        public int index;

        public void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            ConnectionEventHandler.SessionDisconnected += OnSessionDisconnect;
        }

        private void OnSessionDisconnect()
        {
            ConnectionEventHandler.SessionDisconnected -= OnSessionDisconnect;

            RemoveRpc(index);
        }

        protected override void OnNetworkSessionSynchronized()
        {
            base.OnNetworkSessionSynchronized();

            Spawn();
        }

        private void Spawn()
        {
            index = GetRandomIndexExcludingSpawned(animalPrefabs.Count);

            SpawnPlayer(index);

            AddRpc(index);
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
            var prefab = animalPrefabs[0];

            var pos = Util.GetCirclePositions(Vector3.zero, spawnedAnimals.Count, 5f, 8);

            prefab.InstantiateAndSpawn(NetworkManager,
                NetworkManager.LocalClientId,
                isPlayerObject: true,
                position: pos,
                rotation: Quaternion.LookRotation((Vector3.zero - pos).normalized));
        }
    }
}