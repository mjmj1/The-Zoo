using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class InformationPopup : MonoBehaviour
    {
        [SerializeField] private GameObject informationPopupPrefab;
        private readonly Queue<GameObject> informationPopups = new();
    
        void Start()
        {
            for (var i = 0; i < 3; i++)
            {
                var item = Instantiate(informationPopupPrefab, transform);
                informationPopups.Enqueue(item);
                item.SetActive(false);
            }
        }
        
        public void GetInformationPopup(string massage = "")
        {
            GameObject item;
            
            if (informationPopups.Count > 0)
            {
                item = informationPopups.Dequeue();
            }
            else
            {
                item = Instantiate(informationPopupPrefab, transform);
            }

            var msg = item.GetComponentInChildren<TextMeshProUGUI>();
            var btn = item.GetComponentInChildren<Button>();

            msg.text = massage;
            btn.onClick.AddListener(ReleaseInformationPopup(item));
            
            item.SetActive(true);
        }

        private UnityAction ReleaseInformationPopup(GameObject item)
        {
            return () =>
            {
                item.SetActive(false);
                informationPopups.Enqueue(item);
            };
        }
    }
}
