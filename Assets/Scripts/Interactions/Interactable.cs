using Scriptable;
using Unity.Netcode;
using UnityEngine;

namespace Interactions
{
    public abstract class Interactable : NetworkBehaviour
    {
        public enum InteractableType
        {
            LeftClick,
            RightClick,
            R,
            F
        }

        public NetworkVariable<bool> targetMission;
        public NetworkVariable<int> maxSpawnCount;

        [SerializeField] private SfxData sfxData;
        [SerializeField] private ParticleSystem vfxData;

        private void Reset()
        {
            targetMission.Value = false;
            maxSpawnCount.Value = 4;
        }

        public override void OnNetworkSpawn()
        {
            targetMission.OnValueChanged += OnTargetMissionChanged;
            maxSpawnCount.OnValueChanged += OnMaxSpawnCountChanged;

            OnTargetMissionChanged(false, targetMission.Value);
            OnMaxSpawnCountChanged(0, 4);
        }

        public override void OnNetworkDespawn()
        {
            targetMission.OnValueChanged -= OnTargetMissionChanged;
            maxSpawnCount.OnValueChanged -= OnMaxSpawnCountChanged;
        }

        private void OnTargetMissionChanged(bool previousValue, bool newValue)
        {
            if (!IsOwner) return;
            targetMission.Value = newValue;
        }

        private void OnMaxSpawnCountChanged(int previousValue, int newValue)
        {
            if (!IsOwner) return;
            maxSpawnCount.Value = newValue;
        }

        public virtual void StartInteract()
        {
            PlayFxRpc();
        }

        public abstract void StopInteract();


        [Rpc(SendTo.Everyone)]
        private void PlayFxRpc()
        {
            AudioManager.Instance.PlaySfx(sfxData.clip, transform.position, sfxData.volume,
                sfxData.pitch);

            vfxData.Play();
        }

        public abstract InteractableType GetInteractableType();
    }
}