using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace UI
{
    public class SessionListManager : MonoBehaviour
    {
        [SerializeField] GameObject sessionItemPrefab;
        [SerializeField] Transform content;
        private Queue<GameObject> sessionItemPool = new();
        
        void Start()
        {
            gameObject.SetActive(false);
            
            for (var i = 0; i < 10; i++)
            {
                var newItem = Instantiate(sessionItemPrefab, content);
                newItem.SetActive(false);
                sessionItemPool.Enqueue(newItem);
            }
        }
        
        void AddItem(bool isLocked, string name, int maxPlayers, int curPlayers)
        {
            GameObject item;
            if (sessionItemPool.Count > 0)
            {
                item = sessionItemPool.Dequeue();
            }
            else
            {
                item = Instantiate(sessionItemPrefab, content);
            }

            var children = item.GetComponentsInChildren<TextMeshProUGUI>();

            children[1].text = name;
            children[2].text = $"{curPlayers}/{maxPlayers}";
            
            item.SetActive(true);
        }

        void RemoveItem(GameObject item)
        {
            item.SetActive(false);
            sessionItemPool.Enqueue(item);
        }

        void RemoveAllItems()
        {
            for (var i = 0; i < content.childCount; i++)
            {
                RemoveItem(content.GetChild(i).gameObject);
            }
        }
        
        async void RefreshSessionList()
        {
            try
            {
                var result = await LobbyService.Instance.QueryLobbiesAsync();
                
                foreach (var session in result.Results)
                {
                    AddItem(false, session.Name, session.MaxPlayers, session.Players.Count);
                }
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public void OnRefreshButtonClick()
        {
            RemoveAllItems();
            
            RefreshSessionList();
        }
    }
}
