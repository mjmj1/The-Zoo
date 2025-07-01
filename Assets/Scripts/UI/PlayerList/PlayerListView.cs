using System.Collections.Generic;
using System.Linq;
using Networks;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Pool;
using static Static.Strings;

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

            foreach (var player in session.Players)
            {
                var item = pool.Get();

                player.Properties.TryGetValue(PLAYERNAME, out var prop);

                var playerName = prop == null ? "UNKNOWN" : prop.Value;
                
                item.SetPlayerName(playerName);
                
                map.Add(player.Id, item);

                if (player.Id == session.CurrentPlayer.Id) item.Highlight();
            }

            session.PlayerJoined += SessionOnPlayerJoined;
            session.PlayerHasLeft += SessionOnPlayerHasLeft;
            session.SessionHostChanged += SessionOnSessionHostChanged;

            map[session.Host].Host();
        }

        private void OnDisable()
        {
            Clear();
            
            session.PlayerJoined -= SessionOnPlayerJoined;
            session.PlayerHasLeft -= SessionOnPlayerHasLeft;
            session.SessionHostChanged -= SessionOnSessionHostChanged;

            session = null;
        }

        private void SessionOnSessionHostChanged(string obj)
        {
            map[obj].Host();
        }

        private void SessionOnPlayerHasLeft(string obj)
        {
            map.Remove(obj, out var player);
            pool.Release(player);
        }

        private void SessionOnPlayerJoined(string obj)
        {
            var item = pool.Get();

            var player = session.Players.First(player => player.Id == obj);
            
            player.Properties.TryGetValue(PLAYERNAME, out var prop);

            var playerName = prop == null ? "UNKNOWN" : prop.Value;
                
            item.SetPlayerName(playerName);
            
            map.Add(obj, item);
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