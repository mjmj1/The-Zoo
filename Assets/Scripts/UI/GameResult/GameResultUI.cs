using System.Collections.Generic;
using GamePlay;
using Players;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Utils;

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

        private void ClearResults()
        {
            for (var i = playersParent.childCount - 1; i >= 0; i--)
                Destroy(playersParent.GetChild(i).gameObject);
        }

        public void OnGameResult(bool isSeekerWin)
        {
            MyLogger.Print(this, "On Game Result");

            ClearResults();

            titleText.text = isSeekerWin
                ? "Seeker Win !"
                : "Hider Win !";

            MyLogger.Print(this,
                $"Seeker Count: {PlayManager.Instance.RoleManager.seekerIds.Count}");
            MyLogger.Print(this,
                $"Hider Count: {PlayManager.Instance.RoleManager.seekerIds.Count}");

            if (isSeekerWin)
            {
                var uniqueIds = new HashSet<ulong>();
                foreach (var seeker in PlayManager.Instance.RoleManager.seekerIds)
                {
                    if (!uniqueIds.Add(seeker)) continue;

                    var item = Instantiate(seekerView, playersParent);

                    var client = NetworkManager.Singleton.ConnectedClients[seeker];
                    var playerName = client
                        .PlayerObject.GetComponent<PlayerEntity>()
                        .playerName.Value.Value;

                    item.SetPlayerName(playerName);
                }
            }
            else
            {
                var uniqueIds = new HashSet<ulong>();
                foreach (var hider in PlayManager.Instance.RoleManager.hiderIds)
                {
                    if (!uniqueIds.Add(hider)) continue;

                    var prefab = PlayManager.Instance.ObserverManager.observerIds.Contains(hider)
                        ? hiderDeath
                        : hiderAlive;

                    var item = Instantiate(prefab, playersParent);
                    var client = NetworkManager.Singleton.ConnectedClients[hider];
                    var playerName = client
                        .PlayerObject.GetComponent<PlayerEntity>()
                        .playerName.Value.Value;

                    item.SetPlayerName(playerName);
                }
            }
        }

        private void OnReturnLobbyButtonClicked()
        {
            GameManager.Instance.GameEndRpc();
        }
    }
}