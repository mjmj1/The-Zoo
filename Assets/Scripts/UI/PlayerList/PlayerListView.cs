using System.Collections.Generic;
using System.Linq;
using Characters;
using Networks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Pool;
using Utils;

namespace UI.PlayerList
{
    public class PlayerListView : MonoBehaviour
    {
        [SerializeField] private GameObject playerViewPrefab;

        private readonly Dictionary<string, PlayerView> _map = new();

        private IObjectPool<PlayerView> _pool;

        private ISession _session;

        private void Awake()
        {
            _pool = new ObjectPool<PlayerView>
            (
                CreatePoolObj,
                GetPoolObj,
                ReleasePoolObj,
                DestroyPoolObj,
                true, 4, 8
            );
        }

        private void OnEnable()
        {
            _session = ConnectionManager.Instance.CurrentSession;

            foreach (var player in _session.Players)
            {
                var item = _pool.Get();

                player.Properties.TryGetValue(Util.PLAYERNAME, out var prop);

                var playerName = prop == null ? "UNKNOWN" : prop.Value;

                item.SetPlayerId(player.Id);
                item.SetPlayerName(playerName);

                _map.Add(player.Id, item);

                if (player.Id == _session.CurrentPlayer.Id) item.Highlight();
            }

            _session.PlayerJoined += OnPlayerJoined;
            _session.PlayerHasLeft += OnPlayerHasLeft;
            _session.SessionHostChanged += OnSessionHostChanged;
            _session.SessionHostChanged += GameManager.Instance.PromotedSessionHost;
            
            _map[_session.Host].Host(true);
        }

        private void OnDisable()
        {
            Clear();

            _session.PlayerJoined -= OnPlayerJoined;
            _session.PlayerHasLeft -= OnPlayerHasLeft;
            _session.SessionHostChanged -= OnSessionHostChanged;
            _session.SessionHostChanged -= GameManager.Instance.PromotedSessionHost;

            _session = null;
        }

        public void OnPlayerReady(string id, bool value)
        {
            _map[id].Ready(value);
        }

        private void OnSessionHostChanged(string obj)
        {
            foreach (var view in _map.Values)
            {
                view.Host(false);
            }

            _map[obj].Host(true);
            _map[obj].Ready(false);
        }

        private void OnPlayerHasLeft(string obj)
        {
            _map.Remove(obj, out var player);
            _pool.Release(player);
        }

        private void OnPlayerJoined(string obj)
        {
            var item = _pool.Get();

            var player = _session.Players.First(player => player.Id == obj);
            
            player.Properties.TryGetValue(Util.PLAYERNAME, out var prop);

            var playerName = prop == null ? "UNKNOWN" : prop.Value;
            
            item.SetPlayerName(playerName);
            
            item.SetPlayerId(obj);
            
            _map.Add(obj, item);
        }

        private void Clear()
        {
            foreach (var kvp in _map)
            {
                _pool.Release(kvp.Value);
            }
            
            _pool.Clear();

            _map.Clear();
        }

        private PlayerView CreatePoolObj()
        {
            return Instantiate(playerViewPrefab, transform).GetComponent<PlayerView>();
        }

        private void GetPoolObj(PlayerView obj)
        {
            obj.gameObject.SetActive(true);
            obj.transform.SetAsLastSibling();
        }

        private void ReleasePoolObj(PlayerView obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void DestroyPoolObj(PlayerView obj)
        {
            Destroy(obj.gameObject);
        }
    }
}