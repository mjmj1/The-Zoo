using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerList
{
    public class PlayerItem : MonoBehaviour
    {
        [SerializeField]
        TMP_Text PlayerNameText;
    
        [SerializeField]
        Image HostIcon;

        public void SetPlayerName(string playerName)
        {
            PlayerNameText.text = playerName;
        }
    }
}
