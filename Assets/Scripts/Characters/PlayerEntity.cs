using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Characters
{
    public class PlayerEntity : NetworkBehaviour
    {
        private readonly NetworkVariable<int> _animalId = new(-1);

        private PlayerPrefabLoader _loader;

        private void Awake()
        {
            _loader = GetComponent<PlayerPrefabLoader>();
            
            _animalId.OnValueChanged += OnAnimalIdChanged;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                var index = Random.Range(0, _loader.Count());
                _animalId.Value = index;

                PrintRpc(NetworkManager.LocalClientId);
            }
            else
            {
                OnAnimalIdChanged(0, _animalId.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            _loader.Unload();
        }

        private void OnAnimalIdChanged(int previousValue, int newValue)
        {
            _loader.Load(transform, newValue);
        }

        [Rpc(SendTo.Everyone)]
        private void PrintRpc(ulong clientId)
        {
            MyLogger.Print(this, $"Client-{clientId}: {_animalId.Value}");
        }
    }
}