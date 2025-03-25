using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Multiplayer;
using UnityEngine;
using static Static.Strings;

namespace UI.PlayerList
{
    public class PlayerList : MonoBehaviour
    {
        [SerializeField] private GameObject playerItemPrefab;
        private readonly Stack<GameObject> _itemPool = new();

        private readonly Dictionary<string, GameObject> _playerDictionary = new();

        private ISession _session;
        
        private void OnEnable()
        {
            _session = GameManager.Instance.connectionManager.Session;

            _session.PlayerJoined += OnPlayerJoined;
            _session.PlayerHasLeft += OnPlayerLeft;

            RefreshPlayerList();
        }

        private void OnDisable()
        {
            Reset();
        }

        private void OnDestroy()
        {
            if (_session == null) return;

            Reset();
        }

        private void OnPlayerJoined(string playerId)
        {
            RefreshPlayerList();
        }

        private void OnPlayerLeft(string playerId)
        {
            RefreshPlayerList();
        }

        private void Reset()
        {
            _session.PlayerJoined -= OnPlayerJoined;
            _session.PlayerHasLeft -= OnPlayerLeft;
        }

        private void RefreshPlayerList()
        {
            if (_session == null) return;

            var players = _session.Players;
            var currentPlayerIds = players.Select(p => p.Id).ToHashSet();

            RemoveObsoletePlayerItems(currentPlayerIds);

            AddOrUpdatePlayerItems(players, _session.Host);

            UIManager.LobbyUIManager.SettingUI();
        }

        private void RemoveObsoletePlayerItems(HashSet<string> currentPlayerIds)
        {
            var toRemove = _playerDictionary.Keys.Except(currentPlayerIds).ToList();

            foreach (var id in toRemove) ReturnPlayerItem(id);
        }

        private void AddOrUpdatePlayerItems(IEnumerable<IReadOnlyPlayer> players, string currentHostId)
        {
            var siblingIndex = 0;

            foreach (var player in players)
            {
                if (!_playerDictionary.ContainsKey(player.Id))
                {
                    var item = GetPlayerItem();
                    _playerDictionary[player.Id] = item;
                }

                var obj = _playerDictionary[player.Id];
                obj.transform.SetSiblingIndex(siblingIndex++);

                if (obj.TryGetComponent<PlayerItem>(out var playerItem))
                {
                    if (player.Properties.TryGetValue(PLAYERNAME, out var nameProperty))
                        playerItem.SetPlayerName(nameProperty.Value);
                    else
                        playerItem.SetPlayerName("Unknown");

                    playerItem.SetHostIconActive(player.Id == currentHostId);
                }
            }
        }

        private GameObject GetPlayerItem()
        {
            if (_itemPool.Count > 0)
            {
                var item = _itemPool.Pop();
                item.SetActive(true);
                return item;
            }

            return Instantiate(playerItemPrefab, transform);
        }

        private void ReturnPlayerItem(string playerId)
        {
            var obj = _playerDictionary[playerId];
            obj.SetActive(false);
            _itemPool.Push(obj);
            _playerDictionary.Remove(playerId);
        }
    }
}