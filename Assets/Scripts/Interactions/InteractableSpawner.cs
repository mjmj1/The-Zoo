using EventHandler;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

namespace Interactions
{
    public class InteractableSpawner : Interactable
    {
        [SerializeField] private NetworkObject spawnObject;
        [SerializeField] private BoxCollider[] spawnPoints;

        private bool isInteracting;

        private readonly List<NetworkObject> spawnedFruit = new();

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
        public void Initailize(bool targeted)
        {
            targetMission.Value = targeted;
        }

        public override void StartInteract()
        {
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

            var fruit = spawnObject.InstantiateAndSpawn(NetworkManager,
                position: spawnPos,
                rotation: Quaternion.identity);
            spawnedFruit.Add(fruit);

            maxSpawnCount.Value--;
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnInteractionRpc(RpcParams rpcParams = default)
        {
            foreach (var obj in spawnedFruit) obj.Despawn();
        }

        public override InteractableType GetInteractableType()
        {
            return InteractableType.LeftClick;
        }
    }
}