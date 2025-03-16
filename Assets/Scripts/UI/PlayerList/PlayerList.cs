using System.Collections.Generic;
using UnityEngine;

namespace UI.PlayerList
{
    public class PlayerList : MonoBehaviour
    {
        [SerializeField]
        GameObject playerItemPrefab;
    
        public IList<GameObject> playerItems = new List<GameObject>();
    
        void Start()
        {
            GameManager.Instance.connectionManager.NetworkManager.SceneManager.OnLoadComplete +=
                (id, sceneName, mode) =>
                {
                    PlayerJoined("asd");
                };
            
            //GameManager.Instance.connectionManager.Session.PlayerJoined += PlayerJoined;
        }
        
        private void PlayerJoined(string playerId)
        {
            foreach (var item in playerItems)
            {
                Destroy(item);
            }
            /*
            var playerPrefab = Instantiate(playerItemPrefab, transform.parent);
                
            if (playerPrefab.TryGetComponent<PlayerItem>(out var playerItem))
            {
                playerItem.SetPlayerName(playerId);
            }
            
            playerItems.Add(playerPrefab);
            */
            /*var players = GameManager.Instance.connectionManager.Session.Players;

            foreach (var player in players)
            {
                var item = Instantiate(playerItemPrefab, transform);
                
                if (item.TryGetComponent<PlayerItem>(out var playerItem))
                {
                    playerItem.SetPlayerName(player.Id);
                }
                
                playerItems.Add(item);
            }*/
        }
    }
}
