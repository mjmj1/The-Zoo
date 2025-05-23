using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay
{
    public class AnimalSelector : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> animalPrefabs = new();

        public static AnimalSelector Instance;
    
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
