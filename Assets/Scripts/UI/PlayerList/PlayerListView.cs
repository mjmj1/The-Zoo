using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Static;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using Utils;

namespace UI.PlayerList
{
    public class PlayerListView : MonoBehaviour
    {
        [SerializeField] private GameObject playerViewPrefab;

        private readonly Dictionary<string, GameObject> _playerMap = new();

        private readonly Queue<GameObject> _pool = new();

        private void OnEnable()
        {
            Initialize();
            RegisterEvent();
        }

        private void OnDisable()
        {
            Clear();
            UnregisterEvent();
        }

        private void RegisterEvent()
        {
            var session = Manage.Session();

            session.PlayerJoined += OnJoined;
            session.PlayerHasLeft += OnLeft;
            session.SessionHostChanged += OnHostChanged;
        }

        private void UnregisterEvent()
        {
            var session = Manage.Session();

            session.PlayerJoined -= OnJoined;
            session.PlayerHasLeft -= OnLeft;
            session.SessionHostChanged -= OnHostChanged;
        }

        private void Initialize()
        {
            var session = Manage.Session();

            foreach (var player in session.Players)
            {
                AddPlayerView(player);
            }
            
            MarkMe();
            
            MarkHost(session.Host);
        }

        private void OnJoined(string obj)
        {
            var session = Manage.Session();

            var player = session.Players.First(x => x.Id.Equals(obj));

            AddPlayerView(player);
        }

        private void OnLeft(string obj)
        {
            RemovePlayerView(obj);
        }

        private void OnHostChanged(string obj)
        {
            MyLogger.Print(this, $"{obj}");

            MarkHost(obj);
        }

        private void MarkHost(string host)
        {
            _playerMap[host].TryGetComponent<PlayerView>(out var view);
            view.MarkHostIcon();
        }

        private void MarkMe()
        {
            _playerMap[Manage.LocalPlayerId()].TryGetComponent<PlayerView>(out var view);
            view.HighlightView();
        }

        private void AddPlayerView(IReadOnlyPlayer player)
        {
            var obj = GetView();
            var view = obj.GetComponent<PlayerView>();

            view.Bind(player);
            _playerMap.Add(player.Id, obj);
        }

        private void RemovePlayerView(string id)
        {
            var obj = _playerMap[id];
            _playerMap.Remove(id);
            ReturnView(obj);
        }

        private void Clear()
        {
            foreach (Transform child in transform) ReturnView(child.gameObject);

            _playerMap.Clear();
        }

        private GameObject GetView()
        {
            GameObject obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
                obj.SetActive(true);
            }
            else
            {
                obj = Instantiate(playerViewPrefab, transform);
            }

            obj.transform.SetAsLastSibling();

            return obj;
        }

        private void ReturnView(GameObject obj)
        {
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}