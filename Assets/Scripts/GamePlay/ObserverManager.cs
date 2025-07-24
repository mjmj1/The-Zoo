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

        [Rpc(SendTo.Authority)]
        public void AddRpc(ulong observerId)
        {
            observerIds.Add(observerId);
        }

        public bool Contains(ulong clientId)
        {
            return observerIds.Contains(clientId);
        }

        private void OnObserverListChanged(NetworkListEvent<ulong> changeEvent)
        {
            var observer = NetworkManager.Singleton.ConnectedClients[changeEvent.Value];

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (observerIds.Contains(client.ClientId))
                {
                    observer.PlayerObject.GetComponent<PlayerEntity>().NetworkShow(client.ClientId);
                    continue;
                }

                observer.PlayerObject.GetComponent<PlayerEntity>().NetworkHide(client.ClientId);
            }


        }
    }
}