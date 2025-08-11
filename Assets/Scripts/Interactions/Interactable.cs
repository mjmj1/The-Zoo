using Unity.Netcode;

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

        public abstract void StartInteract();
        public abstract void StopInteract();

        public abstract InteractableType GetInteractableType();
    }
}