using Players;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay
{
    public class RoleManager : NetworkBehaviour
    {
        internal NetworkList<PlayerData> HiderIds = new();
        internal NetworkList<PlayerData> SeekerIds = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            HiderIds.OnListChanged += OnHiderListChanged;
            SeekerIds.OnListChanged += OnSeekerListChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            HiderIds.OnListChanged -= OnHiderListChanged;
            SeekerIds.OnListChanged -= OnSeekerListChanged;
        }

        private void OnHiderListChanged(NetworkListEvent<PlayerData> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<PlayerData>.EventType.Add) return;

            SetRoleRpc(PlayerEntity.Role.Hider,
                RpcTarget.Single(changeEvent.Value.ClientId, RpcTargetUse.Temp));
        }

        private void OnSeekerListChanged(NetworkListEvent<PlayerData> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<PlayerData>.EventType.Add) return;

            SetRoleRpc(PlayerEntity.Role.Seeker,
                RpcTarget.Single(changeEvent.Value.ClientId, RpcTargetUse.Temp));
        }

        internal void AssignRole()
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            var seeker = Random.Range(0, clients.Count);

            for (var i = 0; i < clients.Count; i++)
            {
                var entity = clients[i].PlayerObject.GetComponent<PlayerEntity>();

                var playerName = entity.playerName.Value;
                var index = entity.animalIndex.Value;

                var data = new PlayerData(clients[i].ClientId, playerName, index);

                if (seeker == i)
                    SeekerIds.Add(data);
                else
                    HiderIds.Add(data);
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