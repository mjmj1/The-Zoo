using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class PlayerPrefabLoader : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> animalPrefabs = new();

        private GameObject _currentAnimal;

        public int Count()
        {
            return animalPrefabs.Count;
        }
        
        public void Load(Transform root, int index)
        {
            if(!_currentAnimal) Unload();
            
            _currentAnimal = Instantiate(animalPrefabs[index], root);
            
            _currentAnimal.transform.localPosition = Vector3.zero;
            _currentAnimal.transform.localRotation = Quaternion.identity;
        }

        public void Unload()
        {
            Destroy(_currentAnimal);
        }
    }
}
