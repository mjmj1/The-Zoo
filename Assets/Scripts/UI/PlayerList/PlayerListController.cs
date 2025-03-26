using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Multiplayer;
using UnityEngine;
using static Static.Strings;

namespace UI.PlayerList
{
    public class PlayerListController : MonoBehaviour
    {
        private ISession _session;
        private PlayerListView _playerListView;

        private void Awake()
        {
            _playerListView = GetComponent<PlayerListView>();    
        }

        private void OnEnable()
        {
            _session = GameManager.Instance.connectionManager.Session;

            _session.PlayerJoined += UpdateView;
            _session.PlayerHasLeft += UpdateView;

            UpdateView("");
        }

        private void OnDisable()
        {
            _session.PlayerJoined -= UpdateView;
            _session.PlayerHasLeft -= UpdateView;
        }


        private void UpdateView(string playerId)
        {
            var sortedPlayers = _session.Players
                .OrderByDescending(p => p.Id == _session.Host)
                .ToList();

            print($"PlayerListController::UpdateView::{sortedPlayers.Count}");
            _playerListView.Players = sortedPlayers;
            
            UIManager.LobbyUIManager.SettingUI();
        }
    }
}