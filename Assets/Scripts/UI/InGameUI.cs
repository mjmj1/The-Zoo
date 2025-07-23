using GamePlay;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button gameEndButton;
        [SerializeField] private GameObject missions;

        private void Start()
        {
            PlayManager.Instance.currentTime.OnValueChanged += OnValueChanged;
            gameEndButton.onClick.AddListener(OnGameEndButtonClicked);
        }

        private void OnDestroy()
        {
            PlayManager.Instance.currentTime.OnValueChanged -= OnValueChanged;
            gameEndButton.onClick.RemoveListener(OnGameEndButtonClicked);
        }

        private void OnValueChanged(int previousValue, int newValue)
        {
            timerText.text = $"{newValue / 60:00}:{newValue % 60:00}";
        }

        private void OnGameEndButtonClicked()
        {
            GameManager.Instance.LoadSceneRpc("Lobby");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                KeyDown_Tab();
            }
        }

        private void KeyDown_Tab()
        {
            if(missions.activeSelf)
                missions.SetActive(false);
            else
                missions.SetActive(true);

            Debug.Log("Tab");
        }
    }
}