using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Static.Strings;

namespace UI.PlayerList
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Image hostIcon;

        public void Set(bool isHost, IReadOnlyPlayer player)
        {
            playerNameText.text =
                player.Properties.TryGetValue(PLAYERNAME, out var nameProperty) ? nameProperty.Value : "Unknown";

            hostIcon.gameObject.SetActive(isHost);
        }
    }
}