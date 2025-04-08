using System;
using System.Collections.Generic;
using System.Linq;
using Networks;
using Static;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Sessions
{
    public class SessionListView : MonoBehaviour
    {
        [SerializeField] private GameObject sessionViewPrefab;
        [SerializeField] private Transform contentParent;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button refreshButton;
        private readonly List<GameObject> items = new();

        
        private readonly List<SessionView> views = new();

        private readonly Queue<SessionView> _pool = new();
        
        private ISessionInfo _selectedSession;
        private IList<ISessionInfo> sessions;

        private void Awake()
        {
            joinButton.onClick.AddListener(OnJoinButtonClick);
            createButton.onClick.AddListener(OnCreateButtonClick);
            refreshButton.onClick.AddListener(OnRefreshButtonClick);
        }

        private void OnEnable()
        {
            joinButton.interactable = false;

            RefreshAsync();
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
            if (_selectedSession == null) return;

            var data = new ConnectionData(ConnectionData.ConnectionType.JoinById, _selectedSession.Id);

            Manage.ConnectionManager().ConnectAsync(data);

            _selectedSession = null;
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
            _selectedSession = sessionInfo;
            if (_selectedSession != null)
                joinButton.interactable = true;
        }

        private SessionView GetSessionView()
        {
            var view = _pool.Dequeue();
            
            if (view == null)
            {
                view = Instantiate(sessionViewPrefab, contentParent).GetComponent<SessionView>();
                _pool.Enqueue(view);
            }

            view.gameObject.SetActive(true);
            return view;
        }

        private void ReturnSessionView(SessionView view)
        {
            view.gameObject.SetActive(false);
            _pool.Enqueue(view);
        }
    }
}