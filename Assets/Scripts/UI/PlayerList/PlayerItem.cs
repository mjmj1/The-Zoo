using TMPro;
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

        public void SetPlayerName(string name)
        {
            PlayerNameText.text = name;
        }
    }
}
