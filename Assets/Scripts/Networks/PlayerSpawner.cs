using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Networks
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private List<NetworkObject> animalPrefabs;

        private List<int> _animalIndexes;
        private int _nextAnimalIndex;

        private void Start()
        {
            _animalIndexes = Enumerable.Range(0, animalPrefabs.Count).ToList();
            _animalIndexes = _animalIndexes.OrderBy(_ => Random.value).ToList();
            _nextAnimalIndex = 0;
        }

        protected override void OnNetworkSessionSynchronized()
        {
            var animalIndex = _animalIndexes[_nextAnimalIndex++];
            var prefab = animalPrefabs[animalIndex];
            
            prefab.InstantiateAndSpawn(NetworkManager,
                NetworkManager.LocalClientId,
                isPlayerObject: true);

            base.OnNetworkSessionSynchronized();
        }
    }
}