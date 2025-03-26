using System;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace UI.PlayerList
{
    public class PlayerListView : MonoBehaviour
    {
        [SerializeField] private GameObject itemPrefab;

        private Action _onPlayersChanged;
        
        private readonly Queue<GameObject> _itemPool = new();
        
        private List<IReadOnlyPlayer> _players = new();
        
        public List<IReadOnlyPlayer> Players
        {
            set
            {
                _players = value;
                Refresh();
                // _onPlayersChanged?.Invoke();
            }
        }

        private void Refresh()
        {
            foreach (Transform child in transform)
            {
                ReturnItem(child.gameObject);
            }
            
            foreach (var player in _players)
            {
                var item = GetItem();
        
                if (item.TryGetComponent<PlayerView>(out var view))
                {
                    var session = GameManager.Instance.connectionManager.Session;
                    view.Set(session.IsHost, player);
                }
            }
        }
        
        private GameObject GetItem()
        {
            if (_itemPool.Count > 0)
            {
                var item = _itemPool.Dequeue();
                item.SetActive(true);
                return item;
            }

            return Instantiate(itemPrefab, transform);
        }
        
        private void ReturnItem(GameObject obj)
        {
            obj.SetActive(false);
            _itemPool.Enqueue(obj);
        }
    }
}
