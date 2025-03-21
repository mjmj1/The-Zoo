using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        [SerializeField] GameObject gameSetupPopup;
        [SerializeField] Button gameSetupButton;
        [SerializeField] Button gameStartButton;
        
        void Start()
        {
            gameSetupButton.onClick.AddListener(OnSettingButtonClick);
            
            if (!GameManager.Instance.connectionManager.Session.IsHost)
            {
                gameSetupButton.gameObject.SetActive(false);
                gameStartButton.GetComponentInChildren<TMP_Text>().text = "Ready";
            }
        }
        
        void OnSettingButtonClick()
        {
            gameSetupPopup.SetActive(!gameSetupPopup.activeSelf);
        }
    }
}
