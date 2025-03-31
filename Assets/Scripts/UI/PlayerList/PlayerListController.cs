using System.Linq;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace UI.PlayerList
{
    public class PlayerListController
    {
        private readonly ISession _session;
        private readonly PlayerListView _view;
        
        private readonly ulong _clientId;

        public PlayerListController(PlayerListView view)
        {
            _session = GameManager.Instance.connectionManager.Session;
            _clientId = NetworkManager.Singleton.LocalClientId;
            
            _view = view;

            foreach (var player in _session.Players)
            {
                _view.AddPlayerView(_session.Host, player);
            }

            _session.PlayerJoined += OnPlayerJoined;
            _session.PlayerHasLeft += OnPlayerLeft;
            NetworkManager.Singleton.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        }

        ~PlayerListController()
        {
            _session.PlayerJoined -= OnPlayerJoined;
            _session.PlayerHasLeft -= OnPlayerLeft;
            NetworkManager.Singleton.OnSessionOwnerPromoted -= OnSessionOwnerPromoted;
        }

        private void OnPlayerJoined(string id)
        {
            var player = _session.Players.FirstOrDefault(p => p.Id == id);
            if (player != null) _view.AddPlayerView(_session.Host, player);
        }

        private void OnPlayerLeft(string id)
        {
            _view.RemovePlayerView(id);
        }

        private void OnSessionOwnerPromoted(ulong ownerId)
        {
            Debug.Log($"OnSessionOwnerPromoted: {ownerId} {_session.Host}");
            _view.PromoteOwner(_session.Host);
        }
    }
}