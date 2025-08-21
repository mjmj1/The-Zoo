using EventHandler;
using GamePlay;
using Players;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Mission
{
    public class MissionManager : NetworkBehaviour
    {
        public static MissionManager instance;

        [SerializeField] internal HiderMissionProgress missionGauge;
        [SerializeField] private TMP_Text seekerMissionCountText;
        [SerializeField] private TMP_Text pickupCountText;
        [SerializeField] private TMP_Text spinCountText;

        public NetworkVariable<int> pickupCount = new();
        public NetworkVariable<int> spinCount = new();
        
        internal readonly int MaxPickup = 15;
        internal readonly int MaxSpin = 60;

        private void Awake()
        {
            if (!instance) instance = this;
            else Destroy(gameObject);

            missionGauge = GetComponent<HiderMissionProgress>();
        }

        public override void OnNetworkSpawn()
        {
            PlayManager.Instance.roleManager.HiderIds.OnListChanged += HiderListChanged;

            pickupCount.OnValueChanged += OnPickupCountChanged;
            spinCount.OnValueChanged += OnSpinCountChanged;

            pickupCountText.text = $"{pickupCount.Value} / {MaxPickup}";
            spinCountText.text = $"{spinCount.Value} / {MaxSpin}";
        }

        public override void OnNetworkDespawn()
        {
            PlayManager.Instance.roleManager.HiderIds.OnListChanged -= HiderListChanged;

            pickupCount.OnValueChanged -= OnPickupCountChanged;
            spinCount.OnValueChanged -= OnSpinCountChanged;
        }

        public int GetTotalMissionCount()
        {
            return MaxPickup + MaxSpin;
        }

        private void HiderListChanged(NetworkListEvent<PlayerData> changeEvent)
        {
            seekerMissionCountText.text = $"{PlayManager.Instance.roleManager.HiderIds.Count}";
        }

        private void OnPickupCountChanged(int prev, int newValue)
        {
            pickupCountText.text = $"{newValue} / {MaxPickup}";
        }

        private void OnSpinCountChanged(int previousValue, int newValue)
        {
            spinCountText.text = $"{spinCount.Value} / {MaxSpin}";
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void AddPickupCountServerRpc(RpcParams rpcParams = default)
        {
            pickupCount.Value += 1;
            missionGauge.HiderProgress.Value += 1;
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void AddSpinCountServerRpc(RpcParams rpcParams = default)
        {
            spinCount.Value += 1;
            missionGauge.HiderProgress.Value += 1;
        }
    }
}