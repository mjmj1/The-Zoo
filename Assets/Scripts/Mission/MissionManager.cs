using ProgressState;
using GamePlay;
using Players;
using Unity.Netcode;
using UnityEngine;
using System;
using TMPro;

namespace Mission
{
    public class MissionManager : NetworkBehaviour
    {
        public static MissionManager instance;

        [SerializeField] private TeamProgressState state;

        public NetworkVariable<int> capturedCount = new();

        public NetworkVariable<int> fruitTotal = new();
        public NetworkVariable<int> fruitCollected = new();

        [SerializeField] private TMP_Text hiderCountText;
        [SerializeField] private TMP_Text missionCountText;

        private void Awake()
        {
            if (!instance) instance = this;
            else Destroy(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            capturedCount.OnValueChanged += OnCapturedChanged;
            
            PlayManager.Instance.RoleManager.HiderIds.OnListChanged += HiderListChanged;

            fruitCollected.OnValueChanged += OnFruitChanged;
            fruitTotal.OnValueChanged += OnFruitChanged;
            
            OnCapturedChanged(0, capturedCount.Value);
            OnFruitChanged(0, fruitCollected.Value);
        }

        private void HiderListChanged(NetworkListEvent<PlayerData> changeEvent)
        {
            hiderCountText.text = PlayManager.Instance.RoleManager.HiderIds.Count.ToString();
        }

        public override void OnNetworkDespawn()
        {
            capturedCount.OnValueChanged -= OnCapturedChanged;
            
            fruitCollected.OnValueChanged -= OnFruitChanged;
            fruitTotal.OnValueChanged -= OnFruitChanged;
        }

        private void OnCapturedChanged(int _, int __)
        {
            if (!state) return;

            //state.SetProgressRpc(TeamRole.Seeker, norm);
        }

        private void OnFruitChanged(int _, int __)
        {
            if (!state) return;

            var total = Mathf.Max(0, fruitTotal.Value);
            float norm = (total > 0) ? (float)fruitCollected.Value / total : 0f;
            state.SetProgressRpc(TeamRole.Hider, norm);

            if (IsServer && PlayManager.Instance.isGameStarted.Value && fruitCollected.Value >= total && total > 0)
            {
                PlayManager.Instance.isGameStarted.Value = false;
                PlayManager.Instance.ShowResultRpc(false);
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void SetTotalsRpc(int hiderInitial, int fruitTotalCount)
        {
            hiderInitial = Mathf.Max(0, hiderInitial);
            fruitTotalCount = Mathf.Max(0, fruitTotalCount);

            fruitTotal.Value = fruitTotalCount;

            capturedCount.Value = 0;
            fruitCollected.Value = 0;

            OnCapturedChanged(0, 0);
            OnFruitChanged(0, 0);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void OnHiderCapturedRpc(RpcParams _ = default)
        {
            //var total = Mathf.Max(0, hiderCount.Value);
            //capturedCount.Value = Mathf.Min(capturedCount.Value + 1, total);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void OnFruitCollectedRpc(RpcParams _ = default)
        {
            var total = Mathf.Max(0, fruitTotal.Value);
            fruitCollected.Value = Mathf.Min(fruitCollected.Value + 1, total);
        }
    }
}