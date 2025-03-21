using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);
            
            if (!GameManager.Instance.connectionManager.NetworkManager.LocalClient.IsSessionOwner)
            {
                gameSetupButton.gameObject.SetActive(false);
                gameStartButton.GetComponentInChildren<TMP_Text>().text = "Ready";
            }
        }
        
        void OnSettingButtonClick()
        {
            gameSetupPopup.SetActive(!gameSetupPopup.activeSelf);
        }

        void OnGameStartButtonClick()
        {
            if (GameManager.Instance.connectionManager.NetworkManager.LocalClient.IsSessionOwner)
            {
                GameManager.Instance.connectionManager.NetworkManager.SceneManager.LoadScene("InGame",
                    LoadSceneMode.Single);
            }
        }
    }
}
