using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UI
{
    public class InformationPopup : MonoBehaviour
    {
        [SerializeField] private GameObject informationPopupPrefab;
        
        private IObjectPool<GameObject> pool;
        
        public static InformationPopup instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            pool = new ObjectPool<GameObject>(
                CreatePoolObj,
                GetPoolObj,
                ReleasePoolObj,
                DestroyPoolObj,
                true, 3, 5
            );
        }

        public void ShowPopup(string massage)
        {
            var item = pool.Get();

            var msg = item.GetComponentInChildren<TextMeshProUGUI>();
            var btn = item.GetComponentInChildren<Button>();

            msg.text = massage;
            
            btn.onClick.AddListener(HidePopup());

            item.SetActive(true);
        }

        private UnityAction HidePopup()
        {
            return () =>
            {
                pool.Release(gameObject);
            };
        }
        
        private GameObject CreatePoolObj()
        {
            return Instantiate(informationPopupPrefab, transform);
        }

        private void GetPoolObj(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void ReleasePoolObj(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void DestroyPoolObj(GameObject obj)
        {
            Destroy(obj);
        }
    }
}