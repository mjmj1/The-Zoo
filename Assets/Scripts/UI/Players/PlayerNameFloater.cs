using Players;
using TMPro;
using UnityEngine;

namespace UI.Players
{
    public class PlayerNameFloater : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private NetworkPlayer player;

        private void Start()
        {
            if (player == null)
                player = GetComponentInParent<NetworkPlayer>();

            playerNameText.text = player.playerName.Value.ToString();

            player.playerName.OnValueChanged += (_, newName) =>
            {
                playerNameText.text = newName.ToString();
            };
        }
    }
}