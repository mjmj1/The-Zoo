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

        private readonly Dictionary<string, PlayerView> map = new();

        private IObjectPool<PlayerView> pool;

        private ISession session;

        public static PlayerListView Instance { get; private set; }

        private void Awake()
        {
            pool = new ObjectPool<PlayerView>
            (
                CreatePoolObj,
                GetPoolObj,
                ReleasePoolObj,
                DestroyPoolObj,
                true, 4, 8
            );

            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            session = ConnectionManager.Instance.CurrentSession;

            foreach (var player in session.Players)
            {
                var item = pool.Get();

                player.Properties.TryGetValue(Util.PLAYERNAME, out var prop);

                var playerName = prop == null ? "UNKNOWN" : prop.Value;

                item.SetPlayerId(player.Id);
                item.SetPlayerName(playerName);

                map.Add(player.Id, item);

                if (player.Id == session.CurrentPlayer.Id) item.Highlight();
            }

            session.PlayerJoined += OnPlayerJoined;
            session.PlayerHasLeft += OnPlayerHasLeft;
            session.SessionHostChanged += OnSessionHostChanged;
            session.SessionHostChanged += GameManager.Instance.PromotedSessionHost;
            
            map[session.Host].Host(true);
        }

        private void OnDisable()
        {
            Clear();

            session.PlayerJoined -= OnPlayerJoined;
            session.PlayerHasLeft -= OnPlayerHasLeft;
            session.SessionHostChanged -= OnSessionHostChanged;
            session.SessionHostChanged -= GameManager.Instance.PromotedSessionHost;

            session = null;
        }

        public void OnPlayerReady(string id, bool value)
        {
            map[id].Ready(value);
        }

        private void OnSessionHostChanged(string obj)
        {
            foreach (var view in map.Values)
            {
                view.Host(false);
            }

            map[obj].Host(true);
            map[obj].Ready(false);
        }

        private void OnPlayerHasLeft(string obj)
        {
            map.Remove(obj, out var player);
            pool.Release(player);
        }

        private void OnPlayerJoined(string obj)
        {
            var item = pool.Get();

            var player = session.Players.First(player => player.Id == obj);
            
            player.Properties.TryGetValue(Util.PLAYERNAME, out var prop);

            var playerName = prop == null ? "UNKNOWN" : prop.Value;
            
            item.SetPlayerName(playerName);
            
            item.SetPlayerId(obj);
            
            map.Add(obj, item);
        }

        private void Clear()
        {
            foreach (var kvp in map)
            {
                pool.Release(kvp.Value);    
            }
            
            pool.Clear();

            map.Clear();
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