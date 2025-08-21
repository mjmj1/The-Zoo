using EventHandler;
using Unity.Netcode;
using UnityEngine;

namespace Mission
{
    public class PickupMission : NetworkBehaviour
    {
        private void OnEnable()
        {
            GamePlayEventHandler.PlayerPickup += OnPlayerPickup;
        }

        private void OnDisable()
        {
            GamePlayEventHandler.PlayerPickup -= OnPlayerPickup;
        }

        private void OnPlayerPickup()
        {
            if (MissionManager.instance != null)
            {
                MissionManager.instance.AddPickupCountServerRpc();
            }
        }
    }
}
