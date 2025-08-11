using System;
using System.Collections.Generic;
using System.Linq;
using Networks;
using Unity.Services.Core;
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
        [SerializeField] private Button closeButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button refreshButton;

        private readonly LinkedList<SessionView> activeViews = new();

        private IObjectPool<SessionView> pool;

        private ISessionInfo selectedSession;

        private void Awake()
        {
            pool = new ObjectPool<SessionView>
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
            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized)) return;

            joinButton.interactable = false;
            RefreshAsync();
            
            closeButton.onClick.AddListener(Toggle);
            joinButton.onClick.AddListener(OnJoinButtonClick);
            createButton.onClick.AddListener(OnCreateButtonClick);
            refreshButton.onClick.AddListener(OnRefreshButtonClick);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(Toggle);
            joinButton.onClick.RemoveListener(OnJoinButtonClick);
            createButton.onClick.RemoveListener(OnCreateButtonClick);
            refreshButton.onClick.RemoveListener(OnRefreshButtonClick);
        }

        private async void OnCreateButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.Create);

            await ConnectionManager.Instance.ConnectAsync(data);
        }

        private async void OnJoinButtonClick()
        {
            if (selectedSession == null) return;

            var data = new ConnectionData(ConnectionData.ConnectionType.JoinById,
                selectedSession.Id);

            await ConnectionManager.Instance.ConnectAsync(data);

            selectedSession = null;
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
                foreach (var view in activeViews) pool.Release(view);

                activeViews.Clear();

                var infos = await ConnectionManager.Instance.QuerySessionsAsync();

                infos = infos.OrderBy<ISessionInfo, object>(s => s.HasPassword).ToList();

                foreach (var info in infos)
                {
                    var view = pool.Get();

                    activeViews.AddLast(view);

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
            selectedSession = sessionInfo;

            if (selectedSession != null)
                joinButton.interactable = true;
        }

        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        private SessionView OnCreatePooledObjects()
        {
            return Instantiate(sessionViewPrefab, contentParent).GetComponent<SessionView>();
        }

        private void OnGetPooledObjects(SessionView sessionView)
        {
            sessionView.gameObject.SetActive(true);
            sessionView.OnSelected.AddListener(OnSelect);
            sessionView.transform.SetAsLastSibling();
        }

        private void OnReturnPooledObjects(SessionView sessionView)
        {
            sessionView.gameObject.SetActive(false);
            sessionView.OnSelected.RemoveAllListeners();
        }

        private void OnDestroyPooledObjects(SessionView sessionView)
        {
            sessionView.OnSelected.RemoveAllListeners();
            
            Destroy(sessionView.gameObject);
        }
    }
}