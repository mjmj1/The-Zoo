using Networks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using Utils;

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