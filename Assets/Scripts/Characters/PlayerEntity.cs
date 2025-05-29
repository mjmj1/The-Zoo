using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Characters
{
    public class PlayerEntity : NetworkBehaviour
    {
        private CharacterNetworkAnimator _networkAnimator;

        private void Awake()
        {
            _networkAnimator = GetComponent<CharacterNetworkAnimator>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            
            SetObjectNameRpc(NetworkManager.LocalClientId);
        }

        [Rpc(SendTo.Everyone)]
        private void SetObjectNameRpc(ulong clientId)
        {
            MyLogger.Print(this, $"Client-{clientId}");
            name = $"Client-{clientId}";
        }

        public void SetTrigger(int id)
        {
            _networkAnimator.SetTrigger(id);
        }

        public void ResetTrigger(int id)
        {
            _networkAnimator.ResetTrigger(id);
        }

        public void SetBool(int id, bool value)
        {
            _networkAnimator.SetBool(id, value);
        }

        public void SetFloat(int id, float value)
        {
            _networkAnimator.SetFloat(id, value);
        }
    }
}