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
            
            GamePlayEventHandler.PlayerPickup += OnPlayerPickup;

            getFoodCountText.text = $"0 / {MaxPickup}";
            spinCountText.text = $"0 / {MaxSpin}";
        }

        public override void OnNetworkDespawn()
        {
            pickupCount.OnValueChanged -= OnPickupCountChanged;
        }
        private void HiderListChanged(NetworkListEvent<PlayerData> changeEvent)
        {
            hiderCountText.text = PlayManager.Instance.RoleManager.HiderIds.Count.ToString();
        }

        private void OnPickupCountChanged(int prev, int newValue)
        {
            missionGauge.HiderProgress.Value += 1;
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
        }

        // 스핀 관련 추가 (OnPickupCountChanged와 같이)
    }
}