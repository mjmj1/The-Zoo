using GamePlay;
using Interactions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

        internal void SetButtonActive(bool isHost)
        {
            returnLobbyButton.interactable = isHost;
        }

        private void ClearResults()
        {
            for (var i = playersParent.childCount - 1; i >= 0; i--)
                Destroy(playersParent.GetChild(i).gameObject);
        }

        public void OnGameResult(bool isSeekerWin)
        {
            ClearResults();

            titleText.text = isSeekerWin
                ? "Seeker Win !"
                : "Hider Win !";

            var uniqueIds = new HashSet<ulong>();

            if (isSeekerWin)
                foreach (var seeker in PlayManager.Instance.RoleManager.SeekerIds)
                {
                    if (!uniqueIds.Add(seeker.ClientId)) continue;

                    var item = Instantiate(seekerView, playersParent);

                    item.SetPlayerName(seeker.Name.Value);
                }
            else
                foreach (var hider in PlayManager.Instance.RoleManager.HiderIds)
                {
                    if (!uniqueIds.Add(hider.ClientId)) continue;

                    var prefab =
                        PlayManager.Instance.ObserverManager.observerIds.Contains(hider.ClientId)
                            ? hiderDeath
                            : hiderAlive;

                    var item = Instantiate(prefab, playersParent);

                    item.SetPlayerName(hider.Name.Value);
                }
        }

        private void OnReturnLobbyButtonClicked()
        {
            StartCoroutine(DelayBeforeEnd());
        }

        private IEnumerator DelayBeforeEnd()
        {
            yield return new WaitForSecondsRealtime(0.1f);

            GameManager.Instance.GameEndRpc();
        }
    }
}