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

        public override void OnNetworkSpawn()
        {
            missionGauge.maxValue = MissionManager.instance.MaxPickup;

            HiderProgress.OnValueChanged += OnHiderProgressChanged;
        }

        private void OnHiderProgressChanged(int previousValue, int newValue)
        {
            SetProgressRpc(newValue);
            percentText.text = $"{newValue / missionGauge.maxValue:0}%";
        }

        [Rpc(SendTo.Everyone)]
        public void SetProgressRpc(int value, RpcParams rpcParams = default)
        {
            missionGauge.value = value;
        }
    }
}
