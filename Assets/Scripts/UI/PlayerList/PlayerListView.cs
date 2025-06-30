using System;
using System.Collections.Generic;
using Characters;
using Networks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Pool;

namespace UI.PlayerList
{
    public class PlayerListView : MonoBehaviour
    {
        [SerializeField] private GameObject playerViewPrefab;

        private readonly Dictionary<string, PlayerView> map = new();

        private IObjectPool<PlayerView> pool;

        private ISession session;

        public event Action OnPlayerSpawned;

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
            session = ConnectionManager.instance.CurrentSession;

            foreach (var player in session.Players)
            {
                var item = pool.Get();

                map.Add(player.Id, item);

                if (player.Id == session.CurrentPlayer.Id)
                {
                    item.Highlight();
                }
            }

            map[session.Host].Host();

            session.PlayerJoined += SessionOnPlayerJoined;
            session.PlayerHasLeft += SessionOnPlayerHasLeft;
            session.SessionHostChanged += SessionOnSessionHostChanged;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        private void OnDisable()
        {
            Clear();
        }

        public void AddPlayerView(string playerId, string playerName)
        {
            var item = pool.Get();

            map.Add(playerId, item);

            item.SetPlayerName(playerName);
        }

        private void SessionOnSessionHostChanged(string obj)
        {
            print($"{obj} is Host");

            map[obj].Host();
        }

        private void SessionOnPlayerHasLeft(string obj)
        {
            print($"{obj} has Left");

            map.Remove(obj, out var player);
            pool.Release(player);

            session.PlayerJoined -= SessionOnPlayerJoined;
            session.PlayerHasLeft -= SessionOnPlayerHasLeft;
            session.SessionHostChanged -= SessionOnSessionHostChanged;

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        private void SessionOnPlayerJoined(string obj)
        {
            print($"{obj} Joined");

            var item = pool.Get();

            map.Add(obj, item);
        }

        private void OnClientConnected(ulong clientId)
        {
            print($"client-{clientId} OnClientConnected");
        }

        private void Clear()
        {
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