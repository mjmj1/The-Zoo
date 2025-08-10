using GamePlay;
using Players;
using Scriptable;
using System.Collections;
using EventHandler;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image[] redHealth;
        [SerializeField] private HpImageData hpImageData;
        [SerializeField] private Image hitOverlay;
        [SerializeField] private float fadeDuration = 0.2f;

        private void Start()
        {
            PlayManager.Instance.currentTime.OnValueChanged += OnTimerChanged;
            NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<Hittable>().health.OnValueChanged += OnPlayerHealthChanged;

            NetworkManager.OnDestroying += OnDestroying;

            GamePlayEventHandler.OnUIChanged("InGame");
        }

        private void OnDestroy()
        {
            NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<Hittable>().health.OnValueChanged -= OnPlayerHealthChanged;
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