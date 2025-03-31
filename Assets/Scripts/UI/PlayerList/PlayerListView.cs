using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace UI.PlayerList
{
    public class PlayerListView : MonoBehaviour
    {
        [SerializeField] private GameObject playerViewPrefab;

        private readonly Dictionary<string, GameObject> _playerMap = new();

        private readonly Queue<GameObject> _pool = new();

        private PlayerListController _controller;
        
        private void OnEnable()
        {
            _controller = new PlayerListController(this);
        }

        private void OnDisable()
        {
            Clear();
        }

        public void AddPlayerView(string hostId, IReadOnlyPlayer player)
        {
            var obj = GetView();

            obj.GetComponent<PlayerView>().Bind(hostId, player);

            _playerMap.Add(player.Id, obj);
        }

        public void RemovePlayerView(string id)
        {
            var obj = _playerMap[id];
            _playerMap.Remove(id);
            ReturnView(obj);
        }

        public void PromoteOwner(string ownerId)
        {
            foreach (var player in _playerMap.Values)
            {
                player.GetComponent<PlayerView>().SetHost(ownerId);    
            }
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