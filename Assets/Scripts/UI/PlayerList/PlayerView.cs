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
        [SerializeField] private Image stateIcon;
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private GameObject actionButtons;
        private string _hostId;

        private string _playerId;

        private void Start()
        {
            actionButtons.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;

            if (_playerId.Equals(_hostId)) return;

            actionButtons.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            actionButtons.SetActive(false);
        }

        public void Set(string hostId, IReadOnlyPlayer player)
        {
            _playerId = player.Id;
            _hostId = hostId;

            playerNameText.text =
                player.Properties.TryGetValue(PLAYERNAME, out var nameProperty) ? nameProperty.Value : "Unknown";

            if (!_playerId.Equals(hostId))
            {
                stateIcon.sprite = null;
                stateIcon.color = new Color32(255, 255, 255, 0);
            }
        }
    }
}