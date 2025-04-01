using System.Collections.Generic;
using System.Linq;
using Static;
using Unity.Services.Multiplayer;
using UnityEngine;


namespace UI.PlayerList
{
    public class PlayerListView : MonoBehaviour
    {
        [SerializeField] private GameObject playerViewPrefab;

        private readonly Dictionary<string, GameObject> _playerMap = new();

        private readonly Queue<GameObject> _pool = new();

        private void OnEnable()
        {
            var session = Manage.Session();

            session.PlayerJoined += OnPlayerJoined;
            session.PlayerHasLeft += OnPlayerLeft;

            foreach (var player in session.Players) AddPlayerView(player);

            _playerMap[session.Host].GetComponent<PlayerView>().SetHost();
        }

        private void OnDisable()
        {
            Clear();
        }

        private void OnPlayerJoined(string obj)
        {
            print($"Player {obj} was join in the session");

            var session = Manage.Session();

            var joined = session.Players.FirstOrDefault(x => x.Id == obj);

            AddPlayerView(joined);
        }

        private void OnPlayerLeft(string obj)
        {
            print($"Player {obj} was left in the session");

            RemovePlayerView(obj);

            var session = Manage.Session();

            _playerMap[session.Host].GetComponent<PlayerView>().SetHost();
        }

        private void AddPlayerView(IReadOnlyPlayer player)
        {
            print($"PlayerView Added {player.Id}");

            var obj = GetView();

            obj.GetComponent<PlayerView>().Bind(player);

            _playerMap.Add(player.Id, obj);
        }

        private void RemovePlayerView(string id)
        {
            print($"PlayerView Removed {id}");
            var obj = _playerMap[id];
            _playerMap.Remove(id);
            ReturnView(obj);
        }

        private void Clear()
        {
            var session = Manage.Session();

            session.PlayerJoined -= OnPlayerJoined;
            session.PlayerHasLeft -= OnPlayerLeft;

            foreach (Transform child in transform) ReturnView(child.gameObject);

            _playerMap.Clear();
        }

        private GameObject GetView()
        {
            GameObject obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
                obj.SetActive(true);
            }
            else
            {
                obj = Instantiate(playerViewPrefab, transform);
            }

            obj.transform.SetAsLastSibling();

            return obj;
        }

        private void ReturnView(GameObject obj)
        {
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}