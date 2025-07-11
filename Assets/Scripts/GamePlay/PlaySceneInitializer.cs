using Unity.Netcode;
using UnityEngine;

namespace GamePlay
{
    public class PlaySceneInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject interactionControllerPrefab;

        private void Start()
        {
            if (!NetworkManager.Singleton.IsConnectedClient) return;

            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;

            var controller = Instantiate(interactionControllerPrefab);
            controller.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}