using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Players
{
    public class NetworkPlayer : NetworkBehaviour
    {
        private void Awake()
        {
            NetworkManager.SceneManager.OnLoadComplete += ConnectFollowCamera;
        }
        
        private void ConnectFollowCamera(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
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
