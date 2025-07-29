using GamePlay;
using Players;
using Scriptable;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject missions;
        [SerializeField] private Image[] redHealth;
        [SerializeField] private HpImageData hpImageData;

        private void Start()
        {
            PlayManager.Instance.currentTime.OnValueChanged += OnTimerChanged;
            NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerEntity>().health.OnValueChanged += OnPlayerHealthChanged;

            NetworkManager.OnDestroying += OnDestroying;
        }

        private void OnDestroy()
        {
            NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerEntity>().health.OnValueChanged -= OnPlayerHealthChanged;
        }

        private void OnDestroying(NetworkManager obj)
        {
            NetworkManager.OnDestroying -= OnDestroying;

            PlayManager.Instance.currentTime.OnValueChanged -= OnTimerChanged;
        }

        private void OnTimerChanged(int previousValue, int newValue)
        {
            timerText.text = $"{newValue / 60:00}:{newValue % 60:00}";
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
            if (missions != null)
            {
                missions.SetActive(!missions.activeSelf);
            }
        }

        private void OnPlayerHealthChanged(int oldValue, int newValue)
        {
            var value = newValue;

            foreach (var item in redHealth)
            {
                item.sprite = value-- > 0 ? hpImageData.hpSprites[1] : hpImageData.hpSprites[0];
            }
        }
    }
}