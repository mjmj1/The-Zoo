using System;
using EventHandler;
using GamePlay;
using Mission;
using Players;
using Scriptable;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject seekerMissionsView;
        [SerializeField] private GameObject hiderMissionsView;
        [SerializeField] private Image[] redHealth;
        [SerializeField] private HpImageData hpImageData;
        [SerializeField] private KeyUI keyUI;
        [SerializeField] private Image hitOverlay;
        [SerializeField] private float fadeDuration = 0.2f;

        private PlayerEntity _localPlayer;

        [SerializeField] private TextMeshProUGUI seekerMissionText;
        [SerializeField] private TextMeshProUGUI hiderMissionText;

        [SerializeField] private Mission.HiderMissionProgress state;
        [SerializeField] private Mission.MissionManager mission;

        private void Start()
        {
            _localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerEntity>();

            PlayManager.Instance.currentTime.OnValueChanged += OnTimerChanged;

            NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<Hittable>().health.OnValueChanged += OnPlayerHealthChanged;

            var input = NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerController>().Input;

            input.InputActions.UI.Tab.performed += OnTabKeyPressed;
            input.InputActions.UI.Tab.canceled += OnTabKeyPressed;

            GamePlayEventHandler.CheckInteractable += OnKeyUI;

            seekerMissionsView.SetActive(false);
            hiderMissionsView.SetActive(false);
            keyUI.gameObject.SetActive(false);
            GamePlayEventHandler.OnUIChanged("InGame");
        }

        private void OnDisable()
        {
            PlayManager.Instance.currentTime.OnValueChanged -= OnTimerChanged;

            var input = NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerController>().Input;

            input.InputActions.UI.Tab.performed -= OnTabKeyPressed;
            input.InputActions.UI.Tab.canceled -= OnTabKeyPressed;

            GamePlayEventHandler.CheckInteractable -= OnKeyUI;

            NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<Hittable>().health.OnValueChanged -= OnPlayerHealthChanged;

        }

        private void OnKeyUI(bool value, bool isTarget, int count)
        {
            keyUI.gameObject.SetActive(value);

            if (!isTarget)
            {
                keyUI.NonInteractable();
            }
            else
            {
                if (count == 0)
                {
                    keyUI.Unable();
                }
                else
                {
                    keyUI.Interactable();
                }
            }
        }

        private void OnTimerChanged(int previousValue, int newValue)
        {
            timerText.text = $"{newValue / 60:00}:{newValue % 60:00}";
        }

        private void OnTabKeyPressed(InputAction.CallbackContext ctx)
        {
            bool isHider = _localPlayer.role.Value == PlayerEntity.Role.Hider;

            if (isHider)
            {
                hiderMissionsView.SetActive(ctx.performed);
            }
            else
            {
                seekerMissionsView.SetActive(ctx.performed);
            }

        }

        private void OnPlayerHealthChanged(int oldValue, int newValue)
        {
            var value = newValue;

            foreach (var item in redHealth)
                item.sprite = value-- > 0 ? hpImageData.hpSprites[1] : hpImageData.hpSprites[0];

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

            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                var alpha = Mathf.Lerp(0.5f, 0, elapsed / fadeDuration);
                hitOverlay.color = new Color(1, 0, 0, alpha);

                yield return null;
            }

            hitOverlay.color = new Color(1, 0, 0, 0);
        }
    }
}