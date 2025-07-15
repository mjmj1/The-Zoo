using System.Collections.Generic;
using System.Linq;
using Characters;
using EventHandler;
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

        private static readonly Dictionary<string, PlayerView> Map = new();

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
            if (!ConnectionManager.Instance) return;
            
            _session = ConnectionManager.Instance.CurrentSession;

            foreach (var player in _session.Players)
            {
                var item = _pool.Get();

                player.Properties.TryGetValue(Util.PLAYERNAME, out var prop);

                var playerName = prop == null ? "UNKNOWN" : prop.Value;

                item.SetPlayerId(player.Id);
                item.SetPlayerName(playerName);

                Map.Add(player.Id, item);

                if (player.Id == _session.CurrentPlayer.Id) item.Highlight();
            }

            GamePlayEventHandler.OnPlayerReady += OnPlayerReady;
            _session.PlayerJoined += OnPlayerJoined;
            _session.PlayerHasLeft += OnPlayerHasLeft;
            _session.SessionHostChanged += OnSessionHostChanged;
            _session.SessionHostChanged += GameManager.Instance.PromotedSessionHost;

            Map[_session.Host].Host(true);
        }

        private void OnDisable()
        {
            Clear();

            GamePlayEventHandler.OnPlayerReady -= OnPlayerReady;
            _session.PlayerJoined -= OnPlayerJoined;
            _session.PlayerHasLeft -= OnPlayerHasLeft;
            _session.SessionHostChanged -= OnSessionHostChanged;
            _session.SessionHostChanged -= GameManager.Instance.PromotedSessionHost;

            _session = null;
        }

        private void OnPlayerReady(ulong clientId, bool obj)
        {
            print($"{clientId} is Ready {obj}");
        }
        
        public void OnPlayerReady(string id, bool value)
        {
            Map[id].Ready(value);
        }

        private void OnSessionHostChanged(string obj)
        {
            foreach (var view in Map.Values)
            {
                view.Host(false);
            }

            Map[obj].Host(true);
            Map[obj].Ready(false);
        }

        private void OnPlayerHasLeft(string obj)
        {
            Map.Remove(obj, out var player);
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
            
            Map.Add(obj, item);
        }

        private void Clear()
        {
            foreach (var kvp in Map)
            {
                _pool.Release(kvp.Value);
            }
            
            _pool.Clear();

            Map.Clear();
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