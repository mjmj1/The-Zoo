using System;
using System.Collections.Generic;
using System.Linq;
using Networks;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Utils;

namespace UI.SessionList
{
    public class SessionListView : MonoBehaviour
    {
        [SerializeField] private GameObject sessionViewPrefab;
        [SerializeField] private Transform contentParent;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button refreshButton;

        private readonly LinkedList<SessionView> _activeViews = new();

        private IObjectPool<SessionView> _pool;

        private ISessionInfo _selectedSession;

        private void Awake()
        {
            joinButton.onClick.AddListener(OnJoinButtonClick);
            createButton.onClick.AddListener(OnCreateButtonClick);
            refreshButton.onClick.AddListener(OnRefreshButtonClick);

            _pool = new ObjectPool<SessionView>
            (
                OnCreatePooledObjects,
                OnGetPooledObjects,
                OnReturnPooledObjects,
                OnDestroyPooledObjects,
                true, 5, 100
            );
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

            ConnectionManager.Instance.ConnectAsync(data);
        }

        private void OnJoinButtonClick()
        {
            if (_selectedSession == null) return;

            var data = new ConnectionData(ConnectionData.ConnectionType.JoinById,
                _selectedSession.Id);

            ConnectionManager.Instance.ConnectAsync(data);

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
                foreach (var view in _activeViews) _pool.Release(view);

                _activeViews.Clear();

                var infos = await ConnectionManager.Instance.QuerySessionsAsync();

                infos = infos.OrderBy<ISessionInfo, object>(s => s.HasPassword).ToList();

                foreach (var info in infos)
                {
                    var view = _pool.Get();

                    _activeViews.AddLast(view);

                    view.Bind(info);
                }
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private void OnSelect(ISessionInfo sessionInfo)
        {
            _selectedSession = sessionInfo;

            if (_selectedSession != null)
                joinButton.interactable = true;
        }

        private void OnDeselect()
        {
            joinButton.interactable = false;
        }

        private SessionView OnCreatePooledObjects()
        {
            return Instantiate(sessionViewPrefab, contentParent).GetComponent<SessionView>();
        }

        private void OnGetPooledObjects(SessionView sessionView)
        {
            sessionView.gameObject.SetActive(true);
            sessionView.OnSelected.AddListener(OnSelect);
            sessionView.OnDeselected.AddListener(OnDeselect);
            sessionView.transform.SetAsLastSibling();
        }

        private void OnReturnPooledObjects(SessionView sessionView)
        {
            sessionView.gameObject.SetActive(false);
            sessionView.OnSelected.RemoveAllListeners();
            sessionView.OnDeselected.RemoveAllListeners();
        }

        private void OnDestroyPooledObjects(SessionView sessionView)
        {
            sessionView.OnSelected.RemoveAllListeners();
            sessionView.OnDeselected.RemoveAllListeners();
            
            Destroy(sessionView.gameObject);
        }
    }
}