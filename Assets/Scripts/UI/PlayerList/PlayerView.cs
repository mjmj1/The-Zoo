using Static;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Static.Strings;

namespace UI.PlayerList
{
    public class PlayerView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite hostSprite;

        [SerializeField] private Image stateIcon;
        [SerializeField] private TMP_Text playerNameText;

        [SerializeField] private GameObject actionButtons;
        [SerializeField] private Button promoteHostButton;
        [SerializeField] private Button kickButton;
        private IReadOnlyPlayer _data;
        private bool _isHost;

        private void Start()
        {
            actionButtons.SetActive(false);
            promoteHostButton.onClick.AddListener(OnPromoteHostButtonClick);
            kickButton.onClick.AddListener(OnKickButtonClick);
        }

        private void OnEnable()
        {
            stateIcon.sprite = null;
            SetAlpha(0f);
            _isHost = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;

            if (_isHost) return;

            actionButtons.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;

            actionButtons.SetActive(false);
        }

        public void Bind(IReadOnlyPlayer data)
        {
            _data = data;

            playerNameText.text =
                _data.Properties.TryGetValue(PLAYERNAME, out var nameProperty) ? nameProperty.Value : "Unknown";
        }

        public void MarkHostIcon()
        {
            stateIcon.sprite = hostSprite;
            SetAlpha(255f);
            _isHost = true;
        }
        
        public void HighlightView()
        {
            playerNameText.text += " [ME]";
            playerNameText.color = Color.cyan;
        }

        private void SetAlpha(float alpha)
        {
            var color = stateIcon.color;
            color.a = alpha;
            stateIcon.color = color;
        }

        private void OnPromoteHostButtonClick()
        {
            GameManager.Instance.connectionManager.ChangeHostAsync(_data.Id);
        }

        private void OnKickButtonClick()
        {
            GameManager.Instance.connectionManager.KickPlayerAsync(_data.Id);
        }
    }
}