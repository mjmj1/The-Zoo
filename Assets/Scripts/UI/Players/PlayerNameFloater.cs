using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace UI.Players
{
    public class PlayerNameFloater : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;

        [Rpc(SendTo.Everyone)]
        public void SetName(string playerName)
        {
            playerNameText.text = playerName;
        }
    }
}