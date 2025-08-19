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
        [SerializeField] private float spawnPadding = 1.5f;
        [SerializeField] private LayerMask groundMask = ~0;

        internal readonly List<NetworkObject> SpawnedInteractions = new();

        private readonly HashSet<int> targetSet = new();

        private void Start()
        {
            if (!IsOwner) return;

            SpawnInteractionObjectsRpc(0, interactionsCount);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SpawnInteractionObjectsRpc(int index, int count, RpcParams rpcParams = default)
        {
            var world = TorusWorld.Instance;
            if (!world) return;

            while (targetCount > 0)
            {
                var value = Random.Range(0, count);
                if (targetSet.Add(value)) targetCount--;
            }

            var prefab = interactionPrefabs[index];

            for (var i = 0; i < count; i++)
            {
                var spawnPos = GetRandomSpawnInTorus(world);
                var randomYaw = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                var interaction = prefab.InstantiateAndSpawn(NetworkManager,
                    position: spawnPos,
                    rotation: randomYaw);

                SpawnedInteractions.Add(interaction);

                interaction.GetComponent<InteractableSpawner>().Initialize(targetSet.Contains(i));
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnInteractionRpc(RpcParams rpcParams = default)
        {
            foreach (var obj in SpawnedInteractions.Where(obj => obj.IsSpawned))
            {
                obj.GetComponent<InteractableSpawner>().DespawnInteraction();
                obj.Despawn();
            }

            SpawnedInteractions.Clear();
        }

        private Vector3 GetRandomSpawnInTorus(TorusWorld world)
        {
            var x = Random.Range(-world.HalfX + spawnPadding, world.HalfX - spawnPadding);
            var z = Random.Range(-world.HalfZ + spawnPadding, world.HalfZ - spawnPadding);

            const float rayHeight = 100f;
            var rayOrigin = new Vector3(x, rayHeight, z);

            return world.WrapXZ(Physics.Raycast(rayOrigin, Vector3.down, out var hit,
                rayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore)
                ? hit.point : new Vector3(x, 0f, z));
        }
    }
}