using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
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
            var session = Manage.Session();
            
            session.PlayerJoined += OnJoined;
            session.PlayerHasLeft += OnLeft;
            session.SessionHostChanged += OnHostChanged;

            /*foreach (var player in session.Players)
            {
                AddPlayerView(player);
            }

            MarkHost(session.Host);
            
            MarkMe();*/
        }

        private void OnDisable()
        {
            Clear();
        }
        
        private void OnJoined(string obj)
        {
            MyLogger.Print(this, $"{obj}");
        }

        private void OnLeft(string obj)
        {
            MyLogger.Print(this, $"{obj}");
        }
        
        private void OnHostChanged(string obj)
        {
            MyLogger.Print(this, $"{obj}");
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
            var session = Manage.Session();

            session.PlayerJoined -= OnJoined;
            session.PlayerHasLeft -= OnLeft;

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