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

        [SerializeField] private TMP_Text playerNameText;

        [SerializeField] private GameObject actionButtons;
        [SerializeField] private Button promoteHostButton;
        [SerializeField] private Button kickButton;

        private bool isHost;

        private string playerId;

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

            isHost = false;

            hostIcon.SetActive(false);

            readyIcon.SetActive(false);

            playerNameText.color = Color.white;
        }

        private void OnDisable()
        {
            promoteHostButton.onClick.RemoveListener(OnPromoteHostButtonClick);
            kickButton.onClick.RemoveListener(OnKickButtonClick);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;

            if (isHost) return;

            actionButtons.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;

            actionButtons.SetActive(false);
        }

        public void SetPlayerId(string pId)
        {
            playerId = pId;
        }

        public void SetPlayerName(string playerName)
        {
            playerNameText.SetText(playerName);
        }

        public void Host(bool value)
        {
            hostIcon.SetActive(value);

            isHost = value;
        }

        public void Ready(bool value)
        {
            readyIcon.SetActive(value);
        }

        public void Highlight()
        {
            playerNameText.color = Color.green;
        }

        private void OnPromoteHostButtonClick()
        {
            ConnectionManager.Instance.ChangeHostAsync(playerId);
        }

        private void OnKickButtonClick()
        {
            ConnectionManager.Instance.KickPlayerAsync(playerId);
        }
    }
}