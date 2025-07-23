using System.Linq;
using Players;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay
{
    public class ObserverManager : NetworkBehaviour
    {
        public NetworkList<ulong> observerIds = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            observerIds.OnListChanged += OnObserverListChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            observerIds.OnListChanged -= OnObserverListChanged;
        }

        private void OnObserverListChanged(NetworkListEvent<ulong> changeEvent)
        {

        }

        private void UpdateVisibility()
        {
            var all = FindObjectsOfType<PlayerEntity>()
                .Select(p => p.NetworkObject);
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                bool isObs = observerIds.Contains(clientId);
                foreach (var obj in all)
                {
                    bool targetObs = observerIds.Contains(obj.OwnerClientId);
                    if (isObs == targetObs) obj.NetworkShow(clientId);
                    else                   obj.NetworkHide(clientId);
                }
            }
        }
    }
}