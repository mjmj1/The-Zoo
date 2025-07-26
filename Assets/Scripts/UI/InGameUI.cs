using GamePlay;
using TMPro;
using UnityEngine;

namespace UI
{
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;

        private void Start()
        {
            PlayManager.Instance.currentTime.OnValueChanged += OnValueChanged;
        }

        private void OnDestroy()
        {
            PlayManager.Instance.currentTime.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(int previousValue, int newValue)
        {
            timerText.text = $"{newValue / 60:00}:{newValue % 60:00}";
        }
    }
}