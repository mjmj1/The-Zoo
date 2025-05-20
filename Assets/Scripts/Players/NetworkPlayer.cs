using Static;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Static.Strings;

namespace Players
{
    public class NetworkPlayer : NetworkBehaviour
    {
        public NetworkVariable<FixedString64Bytes> playerName = new("");

        private void Awake()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += ConnectFollowCamera;
        }

        private void Start()
        {
            ConnectFollowCamera();

            name = $"Player_{Manage.LocalClient().ClientId}";
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            
            SetupPlayerNameRpc(Manage.Session().CurrentPlayer.Properties[PLAYERNAME].Value);
            
            AnimalSelector.Instance.SetAnimals(transform);
        }
        
        private void ConnectFollowCamera()
        {
            if (!IsOwner) return;

            var cam = FindAnyObjectByType<FollowCamera>();

            if (cam != null) cam.target = transform;
        }

        private void ConnectFollowCamera(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            ConnectFollowCamera();
        }

        [Rpc(SendTo.Authority)]
        private void SetupPlayerNameRpc(string playername)
        {
            playerName.Value = playername;
        }
    }
}