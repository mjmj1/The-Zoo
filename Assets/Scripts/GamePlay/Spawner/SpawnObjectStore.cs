using System.Collections.Generic;
using System.Linq;
using Scriptable;
using UnityEngine;

namespace GamePlay.Spawner
{
    public class SpawnObjectStore : MonoBehaviour
    {
        [SerializeField] private List<AnimalData> animalDataList;

        public static SpawnObjectStore Instance { get; private set; }

        public void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);
        }

        public AnimalData GetAnimalData(AnimalType type)
        {
            var data = animalDataList.Find(d => d.type == type);
            
            return data;
        }

        public int GetLength()
        {
            return animalDataList.Count;
        }
    }
}