using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace GamePlay.Spawner
{
    [DefaultExecutionOrder(-100)]
    public class NpcSpawner : NetworkBehaviour
    {
        public static NpcSpawner Instance { get; private set; }

        [SerializeField] private List<NetworkObject> npcPrefabs;

        private readonly List<NetworkObject> spawnedNpcs = new();

        public void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void SpawnNpcRpc(int index, int count, RpcParams rpcParams = default)
        {
            StartCoroutine(SpawnNpcCoroutine(index, count));
        }

        private IEnumerator SpawnNpcCoroutine(int index, int count)
        {
            var prefab = npcPrefabs[index];

            for (var i = 0; i < count; i++)
            {
                var pos = Vector3.zero;

                if (PlanetGravity.Instance)
                    pos = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());

                var npc = prefab.InstantiateAndSpawn(NetworkManager,
                    position: pos,
                    rotation: Quaternion.LookRotation((Vector3.zero - pos).normalized));

                spawnedNpcs.Add(npc);

                yield return null;
            }

        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnNpcRpc(RpcParams rpcParams = default)
        {
            foreach (var npc in spawnedNpcs)
            {
                if(!npc.IsSpawned) continue;

                npc.Despawn();
            }
        }
    }
}