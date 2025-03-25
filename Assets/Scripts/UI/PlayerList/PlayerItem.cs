using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.PlayerList
{
    public class PlayerItem : MonoBehaviour
    {
        [SerializeField] TMP_Text playerNameText;
    
        [SerializeField] Image hostIcon;

        public void SetPlayerName(string playerName)
        {
            playerNameText.text = playerName;
        }

        public void SetHostIconActive(bool isHost)
        {
            hostIcon.gameObject.SetActive(isHost);
        }
        
    }
}
