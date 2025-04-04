using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Networks;
using Static;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Sessions
{
    public class SessionList : MonoBehaviour
    {
        [SerializeField] private GameObject sessionItemPrefab;

        [SerializeField] private GameObject contentParent;

        [SerializeField] private Button joinSessionButton;

        [SerializeField] private Button createSessionButton;

        [SerializeField] private Button refreshSessionListButton;

        private readonly IList<GameObject> items = new List<GameObject>();

        private ISessionInfo _selectedSessionInfo;
        private IList<ISessionInfo> sessions;

        private void Start()
        {
            RefreshSessionListAsync();

            joinSessionButton.interactable = false;

            joinSessionButton.onClick.AddListener(OnJoinButtonClick);
            createSessionButton.onClick.AddListener(OnCreateButtonClick);
            refreshSessionListButton.onClick.AddListener(OnRefreshButtonClick);
        }

        private void OnDestroy()
        {
            joinSessionButton.onClick.RemoveListener(OnJoinButtonClick);
            createSessionButton.onClick.RemoveListener(OnCreateButtonClick);
            refreshSessionListButton.onClick.RemoveListener(OnRefreshButtonClick);
        }

        private void OnJoinButtonClick()
        {
            if (_selectedSessionInfo == null) return;

            var data = new ConnectionData(ConnectionData.ConnectionType.JoinById, _selectedSessionInfo.Id);

            Manage.ConnectionManager().ConnectAsync(data);

            _selectedSessionInfo = null;
            joinSessionButton.interactable = false;
            gameObject.SetActive(false);
        }

        private void OnCreateButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.Create);

            Manage.ConnectionManager().ConnectAsync(data);
        }

        private void OnRefreshButtonClick()
        {
            RefreshSessionListAsync();
        }

        private async void RefreshSessionListAsync()
        {
            try
            {
                await UpdateSessionsAsync();

                foreach (var listItem in items) Destroy(listItem);

                if (sessions == null)
                    return;

                foreach (var sessionInfo in sessions)
                {
                    var itemPrefab = Instantiate(sessionItemPrefab, contentParent.transform);
                    if (itemPrefab.TryGetComponent<SessionItem>(out var sessionItem))
                    {
                        sessionItem.SetSession(sessionInfo);
                        sessionItem.onSessionSelected.AddListener(SelectSession);
                    }

                    items.Add(itemPrefab);
                }
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private void SelectSession(ISessionInfo sessionInfo)
        {
            _selectedSessionInfo = sessionInfo;
            if (_selectedSessionInfo != null)
                joinSessionButton.interactable = true;
        }

        private async Task UpdateSessionsAsync()
        {
            sessions = await Manage.ConnectionManager().QuerySessionsAsync();
        }
    }
}