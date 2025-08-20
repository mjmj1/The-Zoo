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
        public float spinTimer;

        internal readonly int MaxPickup = 15;
        internal readonly int MaxSpin = 60;

        private int maxHiderCount;

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

            GamePlayEventHandler.PlayerPickup += OnPlayerPickup;
            GamePlayEventHandler.PlayerSpined += OnPlayerSpined;

            pickupCountText.text = $"{pickupCount.Value} / {MaxPickup}";
            spinCountText.text = $"{spinCount.Value} / {MaxSpin}";
        }

        public override void OnNetworkDespawn()
        {
            PlayManager.Instance.roleManager.HiderIds.OnListChanged -= HiderListChanged;

            pickupCount.OnValueChanged -= OnPickupCountChanged;
            spinCount.OnValueChanged -= OnSpinCountChanged;

            GamePlayEventHandler.PlayerPickup -= OnPlayerPickup;
            GamePlayEventHandler.PlayerSpined -= OnPlayerSpined;
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

        private void OnPlayerPickup()
        {
            OnPickupRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void OnPickupRpc(RpcParams param = default)
        {
            pickupCount.Value += 1;
            missionGauge.HiderProgress.Value += 1;
        }

        private void OnSpinCountChanged(int previousValue, int newValue)
        {
            spinCountText.text = $"{spinCount.Value} / {MaxSpin}";
        }

        private void OnPlayerSpined(bool value)
        {
            if (!value)
            {
                spinTimer = 0;
                return;
            }

            spinTimer += Time.deltaTime;

            if (!(spinTimer >= 1.0f)) return;

            spinTimer = 0f;
            OnSpinRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void OnSpinRpc(RpcParams param = default)
        {
            spinCount.Value += 1;
            missionGauge.HiderProgress.Value += 1;
        }
    }
}