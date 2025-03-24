using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject gameSetupPopup;
        [SerializeField] private Button gameSetupButton;
        [SerializeField] private Button gameStartButton;

        private void Start()
        {
            gameSetupButton.onClick.AddListener(OnSetupButtonClick);
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);

            if (GameManager.Instance.connectionManager.NetworkManager.LocalClient.IsSessionOwner) return;

            gameSetupButton.gameObject.SetActive(false);
            gameStartButton.GetComponentInChildren<TMP_Text>().text = "Ready";
        }

        private void OnSetupButtonClick()
        {
            gameSetupPopup.SetActive(!gameSetupPopup.activeSelf);
        }

        private void OnGameStartButtonClick()
        {
            if (GameManager.Instance.connectionManager.NetworkManager.LocalClient.IsSessionOwner)
            {
                GameManager.Instance.connectionManager.NetworkManager.SceneManager.LoadScene("InGame",
                    LoadSceneMode.Single);
            }
        }
    }
}