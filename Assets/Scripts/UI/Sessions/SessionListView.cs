using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Networks;
using Static;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Sessions
{
    public class SessionListView : MonoBehaviour
    {
        [SerializeField] private GameObject sessionViewPrefab;
        [SerializeField] private GameObject contentParent;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button refreshButton;

        private readonly List<GameObject> items = new();

        private ISessionInfo _selectedSessionInfo;
        private IList<ISessionInfo> sessions;

        private void Start()
        {
            RefreshAsync();

            joinButton.interactable = false;

            joinButton.onClick.AddListener(OnJoinButtonClick);
            createButton.onClick.AddListener(OnCreateButtonClick);
            refreshButton.onClick.AddListener(OnRefreshButtonClick);
        }

        private void OnDestroy()
        {
            joinButton.onClick.RemoveListener(OnJoinButtonClick);
            createButton.onClick.RemoveListener(OnCreateButtonClick);
            refreshButton.onClick.RemoveListener(OnRefreshButtonClick);
        }

        private void OnCreateButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.Create);

            Manage.ConnectionManager().ConnectAsync(data);
        }
        
        private void OnJoinButtonClick()
        {
            if (_selectedSessionInfo == null) return;

            var data = new ConnectionData(ConnectionData.ConnectionType.JoinById, _selectedSessionInfo.Id);

            Manage.ConnectionManager().ConnectAsync(data);

            _selectedSessionInfo = null;
            joinButton.interactable = false;
            gameObject.SetActive(false);
        }

        private void OnRefreshButtonClick()
        {
            RefreshAsync();
        }

        private async void RefreshAsync()
        {
            try
            {
                sessions = await Manage.ConnectionManager().QuerySessionsAsync();

                foreach (var listItem in items) Destroy(listItem);

                if (sessions == null)
                    return;

                foreach (var sessionInfo in sessions)
                {
                    var itemPrefab = Instantiate(sessionViewPrefab, contentParent.transform);
                    if (itemPrefab.TryGetComponent<SessionView>(out var sessionView))
                    {
                        sessionView.Bind(sessionInfo);
                        sessionView.onSessionSelected.AddListener(SelectSession);
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
                joinButton.interactable = true;
        }
    }
}