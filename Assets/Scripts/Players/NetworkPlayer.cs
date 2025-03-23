using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Players
{
    public class NetworkPlayer : NetworkBehaviour
    {
        void Awake()
        {
            NetworkManager.SceneManager.OnLoadComplete += ConnectFollowCamera;
        }
        
        void ConnectFollowCamera()
        {
            if (!IsOwner) return;
            
            var cam = FindAnyObjectByType<FollowCamera>();

            if (cam != null)
            {
                cam.target = transform;
            }
        }
        
        void ConnectFollowCamera(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (!IsOwner) return;
            if (OwnerClientId != clientId) return;
            
            var cam = FindAnyObjectByType<FollowCamera>();

            if (cam != null)
            {
                cam.target = transform;
            }
        }
    }
}
