using Unity.Netcode;
using UnityEngine;

namespace GamePlay
{
    public class PlaySceneInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject playManagerPrefab;

        private void Start()
        {
            if (!NetworkManager.Singleton.IsConnectedClient) return;

            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;

            if (FindFirstObjectByType<PlayManager>()) return;

            var obj = Instantiate(playManagerPrefab);
            obj.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}