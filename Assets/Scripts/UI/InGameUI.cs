using System;
using EventHandler;
using GamePlay;
using Players;
using Scriptable;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject missionsView;
        [SerializeField] private Image[] redHealth;
        [SerializeField] private HpImageData hpImageData;
        [SerializeField] private GameObject keyUI;

        private void Start()
        {
            PlayManager.Instance.currentTime.OnValueChanged += OnTimerChanged;
            NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerEntity>().health.OnValueChanged += OnPlayerHealthChanged;

            var input = NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerController>().Input;

            input.InputActions.UI.Tab.performed += OnTabKeyPressed;
            input.InputActions.UI.Tab.canceled += OnTabKeyPressed;

            GamePlayEventHandler.CheckInteractable += OnKeyUI;

            missionsView.SetActive(false);
            keyUI.SetActive(false);
        }

        private void OnDisable()
        {
            PlayManager.Instance.currentTime.OnValueChanged -= OnTimerChanged;

            var input = NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerController>().Input;

            input.InputActions.UI.Tab.performed -= OnTabKeyPressed;
            input.InputActions.UI.Tab.canceled -= OnTabKeyPressed;

            NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerEntity>().health.OnValueChanged -= OnPlayerHealthChanged;
        }
   
        private void OnKeyUI(bool value, bool isTarget, int count)
        {
            keyUI.SetActive(value);

            var background = keyUI.transform.GetChild(0).GetComponent<Image>();

            if (isTarget)
            {
                if (count == 0)
                {
                    background.color = Color.white;
                }
                else
                {
                    background.color = new Color32(150, 255, 150, 255); // Light Green
                }
            }
            else
            {
                background.color = new Color32(255, 150, 150, 255); // Light Red
            } 
        }

        private void OnTimerChanged(int previousValue, int newValue)
        {
            timerText.text = $"{newValue / 60:00}:{newValue % 60:00}";
        }

        private void OnTabKeyPressed(InputAction.CallbackContext ctx)
        {
            missionsView.SetActive(ctx.performed);
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