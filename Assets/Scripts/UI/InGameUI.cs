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

        public GameObject KeyUI;

        public static InGameUI instance;

        private void Awake()
        {
            if (!instance) instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            PlayManager.Instance.currentTime.OnValueChanged += OnTimerChanged;
            NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerEntity>().health.OnValueChanged += OnPlayerHealthChanged;

            var input = NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerController>().Input;

            input.InputActions.UI.Tab.performed += OnTabKeyPressed;
            input.InputActions.UI.Tab.canceled += OnTabKeyPressed;

            NetworkManager.OnDestroying += OnDestroying;

            missionsView.SetActive(false);
        }

        private void OnDestroy()
        {
            var input = NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerController>().Input;

            input.InputActions.UI.Tab.performed -= OnTabKeyPressed;
            input.InputActions.UI.Tab.canceled -= OnTabKeyPressed;
        }

        private void OnDestroying(NetworkManager obj)
        {
            NetworkManager.OnDestroying -= OnDestroying;

            obj.LocalClient.PlayerObject
                .GetComponent<PlayerEntity>().health.OnValueChanged -= OnPlayerHealthChanged;

            PlayManager.Instance.currentTime.OnValueChanged -= OnTimerChanged;
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