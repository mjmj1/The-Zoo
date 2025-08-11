using System;
using EventHandler;
using GamePlay;
using Players;
using Scriptable;
using System.Collections;
using EventHandler;
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
        [SerializeField] private Image hitOverlay;
        [SerializeField] private float fadeDuration = 0.2f;

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

            GamePlayEventHandler.OnUIChanged("InGame");
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

        private void OnKeyUI(bool value)
        {
            keyUI.SetActive(value);
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

            ShowHitEffect();
        }

        public void ShowHitEffect()
        {
            StopAllCoroutines();

            StartCoroutine(Flash());
        }
        private IEnumerator Flash()
        {
            hitOverlay.color = new Color(1, 0, 0, 0.5f);

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.5f, 0, elapsed / fadeDuration);
                hitOverlay.color = new Color(1, 0, 0, alpha);

                yield return null;
            }

            hitOverlay.color = new Color(1, 0, 0, 0);
        }
    }
}