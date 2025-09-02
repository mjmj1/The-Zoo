using System.Collections.Generic;
using System.Linq;
using Maps;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Interactions
{
    public class InteractionController : NetworkBehaviour
    {
        [SerializeField] private List<NetworkObject> interactionPrefabs;
        [SerializeField] private int interactionsCount = 15;
        [SerializeField] internal int targetCount = 5;

        private readonly List<NetworkObject> spawnedInteractions = new();

        private readonly HashSet<int> targetSet = new();

        private void Start()
        {
            if (!IsOwner) return;

            SpawnInteractionObjectsRpc(0, interactionsCount);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SpawnInteractionObjectsRpc(int index, int count, RpcParams rpcParams = default)
        {
            while (targetCount > 0)
            {
                var value = Random.Range(0, count);
                if (targetSet.Add(value)) targetCount--;
            }

            var prefab = interactionPrefabs[index];

            for (var i = 0; i < count; i++)
            {
                var spawnPoint = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());
                var rotationOnSurface = Quaternion.FromToRotation(Vector3.up, spawnPoint.normalized);
                var rotation = rotationOnSurface * Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                var interaction = prefab.InstantiateAndSpawn(NetworkManager,
                    position: spawnPoint,
                    rotation: rotation);

                spawnedInteractions.Add(interaction);

                interaction.GetComponent<InteractableSpawner>().Initialize(targetSet.Contains(i));
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnInteractionRpc(RpcParams rpcParams = default)
        {
            foreach (var obj in spawnedInteractions.Where(obj => obj.IsSpawned))
            {
                obj.GetComponent<InteractableSpawner>().DespawnInteraction();
                obj.Despawn();
            }

            spawnedInteractions.Clear();
        }
    }
}