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

        private HiderMissionProgress missionGauge;

        public NetworkVariable<int> pickupCount = new();

        internal readonly int MaxPickup = 20;

        [SerializeField] private TMP_Text hiderCountText;
        [SerializeField] private TMP_Text missionCountText;

        private void Awake()
        {
            if (!instance) instance = this;
            else Destroy(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            missionGauge = GetComponent<HiderMissionProgress>();

            PlayManager.Instance.RoleManager.HiderIds.OnListChanged += HiderListChanged;

            pickupCount.OnValueChanged += OnPickupCountChanged;
            
            GamePlayEventHandler.PlayerPickup += OnPlayerPickup;

            missionCountText.text = $"0 / {MaxPickup}";
        }

        private void HiderListChanged(NetworkListEvent<PlayerData> changeEvent)
        {
            hiderCountText.text = PlayManager.Instance.RoleManager.HiderIds.Count.ToString();
        }

        public override void OnNetworkDespawn()
        {
            pickupCount.OnValueChanged -= OnPickupCountChanged;
        }

        private void OnPickupCountChanged(int prev, int newValue)
        {
            missionGauge.HiderProgress.Value += 1;

            missionCountText.text = $"{newValue.ToString()} / {MaxPickup}";
        }
        private void OnPlayerPickup()
        {
            OnPickupRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void OnPickupRpc(RpcParams _ = default)
        {
            pickupCount.Value -= 1;
        }

        // 스핀 관련 추가 (OnPickupCountChanged와 같이)
    }
}