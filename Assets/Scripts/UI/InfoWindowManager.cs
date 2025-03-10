using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class InfoWindowManager : MonoBehaviour
    {
        [SerializeField] private GameObject infoWindowPrefab;
        private readonly Queue<GameObject> _infoWindowPool = new();
    
        void Start()
        {
            for (var i = 0; i < 3; i++)
            {
                var item = Instantiate(infoWindowPrefab, transform);
                _infoWindowPool.Enqueue(item);
                item.SetActive(false);
            }
        }

        public void GetInfoWindow(string massage = "")
        {
            GameObject item;
            if (_infoWindowPool.Count > 0)
            {
                item = _infoWindowPool.Dequeue();
            }
            else
            {
                item = Instantiate(infoWindowPrefab, transform);
            }

            var msg = item.GetComponentInChildren<TextMeshProUGUI>();
            var btn = item.GetComponentInChildren<Button>();

            msg.text = massage;
            btn.onClick.AddListener(ReleaseInfoWindow(item));
            
            item.SetActive(true);
        }

        private UnityAction ReleaseInfoWindow(GameObject item)
        {
            return () =>
            {
                item.SetActive(false);
                _infoWindowPool.Enqueue(item);
            };
        }
    }
}
