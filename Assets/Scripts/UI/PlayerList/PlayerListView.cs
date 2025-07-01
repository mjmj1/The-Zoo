using System.Collections.Generic;
using System.Linq;
using Networks;
using Unity.Netcode;
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

            print($"session.Players.Count: {session.Players.Count}");
            
            foreach (var player in session.Players)
            {
                var item = pool.Get();

                player.Properties.TryGetValue(Util.PLAYERNAME, out var prop);

                var playerName = prop == null ? "UNKNOWN" : prop.Value;
                
                item.SetPlayerName(playerName);
                
                map.Add(player.Id, item);

                if (player.Id == session.CurrentPlayer.Id) item.Highlight();
            }

            session.PlayerJoined += OnPlayerJoined;
            session.PlayerHasLeft += OnPlayerHasLeft;
            session.SessionHostChanged += OnSessionHostChanged;
            
            map[session.Host].Host();
        }

        private void OnDisable()
        {
            Clear();

            session.PlayerJoined -= OnPlayerJoined;
            session.PlayerHasLeft -= OnPlayerHasLeft;
            session.SessionHostChanged -= OnSessionHostChanged;

            session = null;
        }

        private void OnSessionHostChanged(string obj)
        {
            map[obj].Host();
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