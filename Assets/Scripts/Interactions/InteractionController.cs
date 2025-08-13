using Mission;
using System;
using System.Collections.Generic;
using UI.GameResult;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Interactions
{
    public class InteractionController : NetworkBehaviour
    {
        [SerializeField] private List<NetworkObject> interactionPrefabs;

        [SerializeField] private int interactionsNumber = 15;

        private readonly List<NetworkObject> spawnedInteractions = new();
        private readonly HashSet<int> targetSet = new();

        [SerializeField] internal int TargetCount = 5;

        private void Start()
        {
            if (!IsOwner) return;

            SpawnInteractionObjectsRpc(0, interactionsNumber);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SpawnInteractionObjectsRpc(int index, int count, RpcParams rpcParams = default)
        {
            MissionManager.instance.fruitTotal = TargetCount * 4;
            print("MissionManager.instance.fruitTotal : " + MissionManager.instance.fruitTotal);

            while (TargetCount > 0)
            {
                var value = UnityEngine.Random.Range(0, count);
                if (targetSet.Add(value))
                {
                    TargetCount--;
                }
            }

            var prefab = interactionPrefabs[index];

            for (var i = 0; i < count; i++)
            {
                var spawnPoint = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());

                var rotationOnSurface = Quaternion.FromToRotation(Vector3.up, spawnPoint.normalized);

                var randomYaw = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);

                var interaction = prefab.InstantiateAndSpawn(NetworkManager,
                    position: spawnPoint,
                    rotation: rotationOnSurface * randomYaw);

                spawnedInteractions.Add(interaction);

                interaction.GetComponent<InteractableSpawner>().Initailize(targetSet.Contains(i));
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnInteractionRpc(RpcParams rpcParams = default)
        {
            foreach (var obj in spawnedInteractions)
            {
                MyLogger.Print(this, "despawn interactions");
                obj.Despawn();
            }
        }
    }
}