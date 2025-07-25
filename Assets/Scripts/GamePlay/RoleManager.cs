using Players;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace GamePlay
{
    public class RoleManager : NetworkBehaviour
    {
        public NetworkList<ulong> hiderIds = new(
            writePerm: NetworkVariableWritePermission.Owner,
            readPerm: NetworkVariableReadPermission.Everyone);
        public NetworkList<ulong> seekerIds = new(
            writePerm: NetworkVariableWritePermission.Owner,
            readPerm: NetworkVariableReadPermission.Everyone);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            hiderIds.OnListChanged += OnHiderListChanged;
            seekerIds.OnListChanged += OnSeekerListChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            hiderIds.OnListChanged -= OnHiderListChanged;
            seekerIds.OnListChanged -= OnSeekerListChanged;
        }

        private void OnHiderListChanged(NetworkListEvent<ulong> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<ulong>.EventType.Add) return;

            SetRoleRpc(PlayerEntity.Role.Hider,
                RpcTarget.Single(changeEvent.Value, RpcTargetUse.Temp));
        }

        private void OnSeekerListChanged(NetworkListEvent<ulong> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<ulong>.EventType.Add) return;

            SetRoleRpc(PlayerEntity.Role.Seeker,
                RpcTarget.Single(changeEvent.Value, RpcTargetUse.Temp));
        }

        internal void AssignRole()
        {
            MyLogger.Print(this, $"assigning role");

            var clients = NetworkManager.Singleton.ConnectedClientsList;
            var seeker = Random.Range(0, clients.Count);

            for (var i = 0; i < clients.Count; i++)
            {
                if (seeker == i)
                {
                    if (seekerIds.Contains(clients[i].ClientId)) continue;
                    seekerIds.Add(clients[i].ClientId);
                }
                else
                {
                    if (hiderIds.Contains(clients[i].ClientId)) continue;
                    hiderIds.Add(clients[i].ClientId);
                }
            }
        }

        internal void UnassignRole()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
                SetRoleRpc(PlayerEntity.Role.None,
                    RpcTarget.Single(client, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SetRoleRpc(PlayerEntity.Role role, RpcParams rpcParams)
        {
            var target = NetworkManager.Singleton
                .LocalClient.PlayerObject.GetComponent<PlayerEntity>();

            target.role.Value = role;
        }
    }
}