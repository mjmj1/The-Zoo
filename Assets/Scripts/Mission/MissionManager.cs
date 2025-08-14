using EventHandler;
using GamePlay;
using Mission;
using Players;
using System;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

namespace Mission
{
    public class MissionManager : NetworkBehaviour
    {
        public static MissionManager instance;

        [SerializeField] internal HiderMissionProgress missionGauge;

        public NetworkVariable<int> pickupCount = new();
        public NetworkVariable<int> spinCount = new();

        internal readonly int MaxPickup = 20;
        internal readonly int MaxSpin = 10;

        [SerializeField] private TMP_Text hiderCountText;
        [SerializeField] private TMP_Text getFoodCountText;
        [SerializeField] private TMP_Text spinCountText;

        private void Awake()
        {
            if (!instance) instance = this;
            else Destroy(gameObject);

            missionGauge = GetComponent<HiderMissionProgress>();
        }

        public override void OnNetworkSpawn()
        {
            PlayManager.Instance.RoleManager.HiderIds.OnListChanged += HiderListChanged;

            pickupCount.OnValueChanged += OnPickupCountChanged;
            spinCount.OnValueChanged += OnSpinCountChanged;
            
            GamePlayEventHandler.PlayerPickup += OnPlayerPickup;
            GamePlayEventHandler.PlayerSpin += OnPlayerSpin;

            getFoodCountText.text = $"0 / {MaxPickup}";
            spinCountText.text = $"0 / {MaxSpin}";
        }

        public override void OnNetworkDespawn()
        {
            pickupCount.OnValueChanged -= OnPickupCountChanged;
            spinCount.OnValueChanged -= OnSpinCountChanged;

            GamePlayEventHandler.PlayerPickup -= OnPlayerPickup;
            GamePlayEventHandler.PlayerSpin -= OnPlayerSpin;
        }
        private void HiderListChanged(NetworkListEvent<PlayerData> changeEvent)
        {
            hiderCountText.text = PlayManager.Instance.RoleManager.HiderIds.Count.ToString();
        }

        private void OnPickupCountChanged(int prev, int newValue)
        {
            
            getFoodCountText.text = $"{newValue.ToString()} / {MaxPickup}";
        }
        private void OnPlayerPickup()
        {
            OnPickupRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void OnPickupRpc(RpcParams _ = default)
        {
            pickupCount.Value += 1;
            missionGauge.HiderProgress.Value += 1;
        }

        private void OnSpinCountChanged(int previousValue, int newValue)
        {
            
            
            spinCountText.text = $"{spinCount.Value.ToString()} / {MaxSpin}";
        }
        private void OnPlayerSpin(int spinTime)
        {
            OnSpinRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void OnSpinRpc(RpcParams _ = default)
        {
            spinCount.Value += 1;
            missionGauge.HiderProgress.Value += 1;
        }
    }
}