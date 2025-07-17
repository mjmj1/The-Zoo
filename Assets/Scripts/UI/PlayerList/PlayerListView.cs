using System.Collections.Generic;
using System.Linq;
using EventHandler;
using Networks;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Pool;
using Utils;

namespace UI.PlayerList
{
    public class PlayerListView : MonoBehaviour
    {
        private static readonly Dictionary<string, PlayerView> Map = new();
        [SerializeField] private GameObject playerViewPrefab;

        private IObjectPool<PlayerView> pool;

        private ISession session;

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
        }

        private void OnEnable()
        {
            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized)) return;

            if (!ConnectionManager.Instance) return;

            session = ConnectionManager.Instance.CurrentSession;

            foreach (var player in session.Players)
            {
                var item = pool.Get();

                player.Properties.TryGetValue(Util.PLAYERNAME, out var prop);

                item.Bind(player);

                Map.Add(player.Id, item);

                if (player.Id == session.CurrentPlayer.Id) item.Highlight();
            }

            GamePlayEventHandler.OnPlayerReady += OnPlayerReady;
            session.PlayerJoined += OnPlayerJoined;
            session.PlayerHasLeft += OnPlayerHasLeft;
            session.SessionHostChanged += OnSessionHostChanged;
            session.SessionHostChanged += GameManager.Instance.PromotedSessionHost;

            Map[session.Host].Host(true);
        }

        private void OnDisable()
        {
            if(session == null) return;

            Clear();

            GamePlayEventHandler.OnPlayerReady -= OnPlayerReady;
            session.PlayerJoined -= OnPlayerJoined;
            session.PlayerHasLeft -= OnPlayerHasLeft;
            session.SessionHostChanged -= OnSessionHostChanged;
            session.SessionHostChanged -= GameManager.Instance.PromotedSessionHost;

            session = null;
        }

        private void OnPlayerReady(string playerId, bool value)
        {
            Map[playerId].Ready(value);
        }

        private void OnSessionHostChanged(string playerId)
        {
            foreach (var view in Map.Values) view.Host(false);

            Map[playerId].Host(true);
            Map[playerId].Ready(false);
        }

        private void OnPlayerHasLeft(string playerId)
        {
            Map.Remove(playerId, out var player);
            pool.Release(player);
        }

        private void OnPlayerJoined(string playerId)
        {
            var item = pool.Get();

            var player = session.Players.First(player => player.Id == playerId);

            item.Bind(player);

            Map.Add(playerId, item);
        }

        private void Clear()
        {
            foreach (var kvp in Map) pool.Release(kvp.Value);

            pool.Clear();

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