using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay
{
    public class GameTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;

        private void Start()
        {
            PlayManager.CurrentTime.OnValueChanged += OnValueChanged;
        }

        private void OnValueChanged(int previousValue, int newValue)
        {
            timerText.text = $"{newValue / 60:00}:{newValue % 60:00}";
        }
    }
}