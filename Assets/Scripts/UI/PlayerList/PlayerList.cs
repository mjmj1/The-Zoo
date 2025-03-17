using System.Collections.Generic;
using Networks;
using Unity.Netcode;
using UnityEngine;
using static Static.Strings;

namespace UI.PlayerList
{
    public class PlayerList : MonoBehaviour
    {
        [SerializeField]
        GameObject playerItemPrefab;
    
        IDictionary<string, GameObject> _playerDict = new Dictionary<string, GameObject>();
        
        void Start()
        {
            InitializePlayers();
            
            GameManager.Instance.connectionManager.Session.PlayerJoined += PlayerJoined;
            GameManager.Instance.connectionManager.Session.PlayerLeaving += PlayerLeft;
        }

        private void InitializePlayers()
        {
            var session = GameManager.Instance.connectionManager.Session;
            
            foreach (var player in session.Players)
            {
                var item = Instantiate(playerItemPrefab, transform);
                
                if (item.TryGetComponent<PlayerItem>(out var playerItem))
                {
                    playerItem.SetPlayerName(player.Properties[PLAYERNAME].Value);
                }
                
                _playerDict.Add(player.Id, item);
            }
        }
        private void PlayerJoined(string playerId)
        {
            var session = GameManager.Instance.connectionManager.Session;

            foreach (var player in session.Players)
            {
                if (player.Id != playerId) continue;
                
                var item = Instantiate(playerItemPrefab, transform);
            
                if (item.TryGetComponent<PlayerItem>(out var playerItem))
                {
                    playerItem.SetPlayerName(player.Properties[PLAYERNAME].Value);
                }
                
                _playerDict.Add(playerId, item);
                    
                return;
            }
        }

        private void PlayerLeft(string playerId)
        {
            var item = _playerDict[playerId];
            Destroy(item);
            _playerDict.Remove(playerId);
        }
    }
}
