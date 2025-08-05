using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Networks
{
    [DefaultExecutionOrder(-100)]
    public class NpcSpawner : NetworkBehaviour
    {
        public static NpcSpawner Instance { get; private set; }

        [SerializeField] private List<NetworkObject> npcPrefabs;

        private List<NetworkObject> spawnedNpcs = new();

        public void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void SpawnNpcRpc(int index, int count, RpcParams rpcParams = default)
        {
            var prefab = npcPrefabs[index];

            for (var i = 0; i < count; i++)
            {
                var pos = Util.GetRandomPositionInSphere(7.5f);

                var npc = prefab.InstantiateAndSpawn(NetworkManager,
                    position: pos,
                    rotation: Quaternion.LookRotation((Vector3.zero - pos).normalized));

                spawnedNpcs.Add(npc);
            }

        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnNpcRpc(RpcParams rpcParams = default)
        {
            foreach (var npc in spawnedNpcs)
            {
                npc.Despawn();
            }
        }
    }
}