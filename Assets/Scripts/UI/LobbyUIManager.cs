using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject gameSetup;
        [SerializeField] private Button gameStartButton;

        private void Start()
        {
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);

            if (GameManager.Instance.connectionManager.NetworkManager.LocalClient.IsSessionOwner) return;

            gameSetup.gameObject.SetActive(false);
            gameStartButton.GetComponentInChildren<TMP_Text>().text = "Ready";
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