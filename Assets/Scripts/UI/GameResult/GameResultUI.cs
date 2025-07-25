using GamePlay;
using Players;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GameResult
{
    public class GameResultUI : MonoBehaviour
    {
        [SerializeField] private Button returnLobbyButton;
        [SerializeField] private Transform playersParent;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private ResultItem hiderAlive;
        [SerializeField] private ResultItem hiderDeath;
        [SerializeField] private ResultItem seekerView;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void Start()
        {
            returnLobbyButton.onClick.AddListener(OnReturnLobbyButtonClicked);
        }

        private void OnDestroy()
        {
            returnLobbyButton.onClick.RemoveListener(OnReturnLobbyButtonClicked);
        }

        public void SetTitle(string title)
        {
            titleText.text = title;
        }

        public void OnGameResult(bool isSeekerWin)
        {
            if (isSeekerWin)
                foreach (var seeker in PlayManager.Instance.RoleManager.seekerIds)
                {
                    var item = Instantiate(seekerView, playersParent).GetComponent<ResultItem>();

                    var client = NetworkManager.Singleton.ConnectedClients[seeker];
                    var playerName = client.PlayerObject.GetComponent<PlayerEntity>().playerName;
                    item.SetPlayerName(playerName.Value.Value);
                }
            else
                foreach (var hider in PlayManager.Instance.RoleManager.hiderIds)
                {
                    var item = PlayManager.Instance.ObserverManager.observerIds.Contains(hider)
                        ? Instantiate(hiderDeath, playersParent).GetComponent<ResultItem>()
                        : Instantiate(hiderAlive, playersParent).GetComponent<ResultItem>();

                    var client = NetworkManager.Singleton.ConnectedClients[hider];
                    var playerName = client.PlayerObject.GetComponent<PlayerEntity>().playerName;
                    item.SetPlayerName(playerName.Value.Value);
                }
        }

        private void OnReturnLobbyButtonClicked()
        {
            GameManager.Instance.GameEndRpc();
        }
    }
}