using Networks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.PlayerList
{
    public class PlayerView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject hostIcon;
        [SerializeField] private GameObject readyIcon;
        [SerializeField] private GameObject otherBg;
        [SerializeField] private GameObject bg;

        [SerializeField] private TMP_Text playerNameText;

        [SerializeField] private GameObject actionButtons;
        [SerializeField] private Button promoteHostButton;
        [SerializeField] private Button kickButton;

        private bool _isHost;

        private string _playerId;

        private void Start()
        {
            actionButtons.SetActive(false);

            promoteHostButton.onClick.AddListener(OnPromoteHostButtonClick);
            kickButton.onClick.AddListener(OnKickButtonClick);
        }

        private void OnEnable()
        {
            actionButtons.SetActive(false);

            promoteHostButton.onClick.AddListener(OnPromoteHostButtonClick);
            kickButton.onClick.AddListener(OnKickButtonClick);

            _isHost = false;

            otherBg.SetActive(true);
            
            bg.SetActive(false);
            
            hostIcon.SetActive(false);

            readyIcon.SetActive(false);
        }

        private void OnDisable()
        {
            promoteHostButton.onClick.RemoveListener(OnPromoteHostButtonClick);
            kickButton.onClick.RemoveListener(OnKickButtonClick);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!ConnectionManager.Instance.CurrentSession.IsHost) return;

            if (_isHost) return;

            actionButtons.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!ConnectionManager.Instance.CurrentSession.IsHost) return;

            actionButtons.SetActive(false);
        }

        public void SetPlayerId(string pId)
        {
            _playerId = pId;
        }

        public void SetPlayerName(string playerName)
        {
            playerNameText.SetText(playerName);
        }

        public void Host(bool value)
        {
            hostIcon.SetActive(value);

            _isHost = value;
            
            actionButtons.SetActive(false);
        }

        public void Ready(bool value)
        {
            readyIcon.SetActive(value);
        }

        public void Highlight()
        {
            otherBg.SetActive(false);
            bg.SetActive(true);
        }

        private void OnPromoteHostButtonClick()
        {
            ConnectionManager.Instance.ChangeHostAsync(_playerId);
        }

        private void OnKickButtonClick()
        {
            ConnectionManager.Instance.KickPlayerAsync(_playerId);
        }
    }
}