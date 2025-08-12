using System;
using System.Collections.Generic;
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

        public int TargetCount = 5; // interactionable tree count

        public event Action OnTargetCompleted;

        public NetworkVariable<int> TargetTotalNv { get; } =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> CompletedTargetsNv { get; } =
            new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private void Start()
        {
            if (!IsOwner) return;

            SpawnInteractionObjectsRpc(0, interactionsNumber);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SpawnInteractionObjectsRpc(int index, int count, RpcParams rpcParams = default)
        {
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

            var total = targetSet.Count * 5;
            TargetTotalNv.Value = total;
            CompletedTargetsNv.Value = 0;
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnInteractionRpc(RpcParams rpcParams = default)
        {
            foreach (var obj in spawnedInteractions) obj.Despawn();
        }

        public int CompletedTargetCount { get; private set; }

        public int RemainingTargetCount => Mathf.Max(0, TargetCount - CompletedTargetCount);

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void ReportTargetCompletedRpc(RpcParams _ = default)
        {
            if (!IsServer) return;

            int cap = Mathf.Max(1, TargetTotalNv.Value);
            CompletedTargetsNv.Value = Mathf.Min(CompletedTargetsNv.Value + 1, cap);

            CompletedTargetCount = Mathf.Min(TargetCount, CompletedTargetCount + 1);
            OnTargetCompleted?.Invoke();

            Debug.Log($"[Server] Completed={CompletedTargetsNv.Value + 1}/{TargetTotalNv.Value}");

        }
    }
}