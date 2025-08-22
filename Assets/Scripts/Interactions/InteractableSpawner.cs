using EventHandler;
using Mission;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Interactions
{
    public class InteractableSpawner : Interactable
    {
        [SerializeField] private Transform visualsRoot;
        [SerializeField] private NetworkObject spawnObject;
        [SerializeField] private BoxCollider[] spawnPoints;
        [SerializeField] private int spawnTypeIndex = 0;

        private bool isInteracting;

        internal readonly List<NetworkObject> SpawnedObject = new();

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (spawnPoints == null) return;

            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Gizmos.color = Color.yellow;

            var halfSize = spawnPoint.size * 0.5f;
            var center = spawnPoint.center;

            // 로컬 그리기: 8개 모서리 구하기
            for (var x = -1; x <= 1; x += 2)
            for (var y = -1; y <= 1; y += 2)
            for (var z = -1; z <= 1; z += 2)
            {
                var localCorner = center + Vector3.Scale(halfSize, new Vector3(x, y, z));
                var worldCorner = spawnPoint.transform.TransformPoint(localCorner);
                Gizmos.DrawSphere(worldCorner, 0.05f);
            }
        }
#endif
        public void Initialize(bool targeted)
        {
            targetMission.Value = targeted;
        }

        public override void StartInteract()
        {
            base.StartInteract();

            if (targetMission.Value)
                if (maxSpawnCount.Value > 0)
                {
                    if (isInteracting) return;

                    isInteracting = true;

                    SpawnRpc();

                    print($"{gameObject.name} is interacting...");
                }
                else
                {
                    GamePlayEventHandler.OnCheckInteractable(true, true, 0);
                }
        }

        public override void StopInteract()
        {
            if (!isInteracting) return;

            isInteracting = false;
            print($"{gameObject.name} is stop interacting...");
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SpawnRpc(RpcParams rpcParams = default)
        {
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            var min = spawnPoint.bounds.min;
            var max = spawnPoint.bounds.max;
            var spawnPos = new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
            );

            var no = spawnObject.InstantiateAndSpawn(NetworkManager,
                position: spawnPos,
                rotation: Quaternion.identity);
            SpawnedObject.Add(no);

            ApplyVariant(no, spawnTypeIndex);
            ApplyVariantClientRpc(no, spawnTypeIndex);

            maxSpawnCount.Value--;
        }

        internal void DespawnInteraction()
        {
            foreach (var obj in SpawnedObject.Where(obj => obj.IsSpawned))
            {
                obj.Despawn();
            }
        }
        public void SetSpawnTypeIndex(int typeIndex)
        {
            spawnTypeIndex = typeIndex;
        }
        public void SetVisualsByIndex(int index)
        {
            if (visualsRoot == null) return;
            for (int i = 0; i < visualsRoot.childCount; i++)
            {
                visualsRoot.GetChild(i).gameObject.SetActive(i == index);
            }
        }
        public override InteractableType GetInteractableType()
        {
            return InteractableType.LeftClick;
        }
        private void ApplyVariant(NetworkObject no, int index)
        {
            var root = no.transform;
            if (root.childCount == 0) return;

            var models = root.GetChild(0);
            for (int i = 0; i < models.childCount; i++)
            {
                var go = models.GetChild(i).gameObject;
                go.SetActive(i == index);
            }
        }
        [Rpc(SendTo.Everyone)]
        private void ApplyVariantClientRpc(NetworkObjectReference objectRef, int index, RpcParams _ = default)
        {
            if (!objectRef.TryGet(out var no)) return;
            ApplyVariant(no, index);
        }
    }
}