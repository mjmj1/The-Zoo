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
            Initialize();

            session.PlayerHasLeft += OnHasLeft;
            session.SessionHostChanged += OnHostChanged;
        }

        private void OnDisable()
        {
            Clear();

            session.PlayerHasLeft -= OnHasLeft;
            session.SessionHostChanged -= OnHostChanged;
        }

        private void Initialize()
        {
            session = ConnectionManager.instance.CurrentSession;

            foreach (var player in session.Players)
            {
                JoinPlayer(player.Id);
            }

            foreach (var player in map)
            {
                print($"{player.Key} - {player.Value}");
            }

            SetPlayerNameRpc(session.CurrentPlayer.Id);

            map[AuthenticationService.Instance.PlayerId].Highlight();

            map[session.Host].Host();
        }

        private void OnJoined(string obj)
        {
            JoinPlayer(obj);

            SetPlayerNameRpc(obj);
        }

        private void OnHasLeft(string obj)
        {
            LeftPlayerRpc(obj);
        }

        private void OnHostChanged(string obj)
        {
            ChangeHostRpc(obj);
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

        private void JoinPlayer(string obj)
        {
            var item = pool.Get();

            map.Add(obj, item);
        }

        [Rpc(SendTo.Everyone)]
        private void SetPlayerNameRpc(string obj)
        {
            if (AuthenticationService.Instance.PlayerId != obj) return;

            var client = NetworkManager.Singleton.LocalClient.PlayerObject;

            var playerName = client.GetComponent<PlayerEntity>().playerName.Value.ToString();

            print($"{playerName}");

            map[obj].SetPlayerName(playerName);
        }

        [Rpc(SendTo.Everyone)]
        private void LeftPlayerRpc(string obj)
        {
            pool.Release(map[obj]);
            map.Remove(obj);
        }

        [Rpc(SendTo.Everyone)]
        private void ChangeHostRpc(string obj)
        {
            map[obj].Host();
        }
    }
}