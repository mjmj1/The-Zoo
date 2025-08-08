using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Networks
{
    [DefaultExecutionOrder(-100)]
    public class NpcSpawner : NetworkBehaviour
    {
        [SerializeField] private List<NetworkObject> npcPrefabs;

        private readonly List<NetworkObject> spawnNpcs = new();
        public static NpcSpawner Instance { get; private set; }

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
                var pos = Util.GetRandomPositionInSphere(7.5f);

                var npc = prefab.InstantiateAndSpawn(NetworkManager,
                    position: pos,
                    rotation: Quaternion.LookRotation((Vector3.zero - pos).normalized));

                spawnNpcs.Add(npc);
                yield return null;
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnNpcRpc(RpcParams rpcParams = default)
        {
            foreach (var npc in spawnNpcs) npc.Despawn();
        }
    }
}