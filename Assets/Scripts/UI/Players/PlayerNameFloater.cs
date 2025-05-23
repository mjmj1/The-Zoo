using Characters;
using TMPro;
using UnityEngine;

namespace UI.Players
{
    public class PlayerNameFloater : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private PlayerData playerData;

        private void Start()
        {
            if (playerData == null)
                playerData = GetComponentInParent<PlayerData>();

            playerNameText.text = playerData.playerName.Value.ToString();

            playerData.playerName.OnValueChanged += (_, newName) =>
            {
                playerNameText.text = newName.ToString();
            };
        }
    }
}