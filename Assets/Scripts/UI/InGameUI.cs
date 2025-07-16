using GamePlay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button gameEndButton;

        private void Awake()
        {
            PlayManager.CurrentTime.OnValueChanged += OnValueChanged;
            gameEndButton.onClick.AddListener(OnGameEndButtonClicked);
        }

        private void OnDestroy()
        {
            PlayManager.CurrentTime.OnValueChanged -= OnValueChanged;
            gameEndButton.onClick.RemoveListener(OnGameEndButtonClicked);
        }

        private void OnValueChanged(int previousValue, int newValue)
        {
            timerText.text = $"{newValue / 60:00}:{newValue % 60:00}";
        }

        private void OnGameEndButtonClicked()
        {
            GameManager.Instance.LoadLobbyScene();
        }
    }
}