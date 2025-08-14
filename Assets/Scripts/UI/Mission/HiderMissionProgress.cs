using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Mission
{

    public class HiderMissionProgress : NetworkBehaviour
    {
        [SerializeField] private Slider missionGauge;
        [SerializeField] private TMP_Text percentText;
        
        public NetworkVariable<int> HiderProgress = new(); // whole mission
        private void Awake()
        {
            percentText.text = "0%";
        }
        public override void OnNetworkSpawn()
        {
            missionGauge.maxValue = MissionManager.instance.MaxPickup + MissionManager.instance.MaxSpin;

            HiderProgress.OnValueChanged += OnHiderProgressChanged;
        }

        private void OnHiderProgressChanged(int previousValue, int newValue)
        {
            SetProgressRpc(newValue);
            var percentage = newValue / missionGauge.maxValue * 100;
            percentText.text = $"{percentage:F0}%";
        }

        [Rpc(SendTo.Everyone)]
        public void SetProgressRpc(int value, RpcParams rpcParams = default)
        {
            missionGauge.value = value;
        }
    }
}
