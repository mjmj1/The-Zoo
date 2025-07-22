using System.Collections;
using System.Linq;
using Characters;
using Characters.Roles;
using Players;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using Utils;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        [SerializeField] private GameObject observerPrefab;
        [SerializeField] private float spawnRadius = 7.5f;

        public static PlayManager Instance { get; private set; }

        public NetworkVariable<int> currentTime = new();
        private readonly WaitForSeconds waitDelay = new(1.0f);

        public NetworkList<ulong> hiderIds = new();
        public NetworkList<ulong> seekerIds = new();

        private bool isGameStarted;

        public void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            hiderIds.OnListChanged += OnHiderListChanged;
            seekerIds.OnListChanged += OnSeekerListChanged;

            if (!IsSessionOwner) return;

            OnGameStart();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            OnGameEnd();
        }

        private void OnHiderListChanged(NetworkListEvent<ulong> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<ulong>.EventType.Add) return;

            SetRoleRpc(PlayerEntity.Role.Hider,
                RpcTarget.Single(changeEvent.Value, RpcTargetUse.Temp));
        }

        private void OnSeekerListChanged(NetworkListEvent<ulong> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<ulong>.EventType.Add) return;

            SetRoleRpc(PlayerEntity.Role.Seeker,
                RpcTarget.Single(changeEvent.Value, RpcTargetUse.Temp));
        }

        private void OnGameStart()
        {
            if (!IsSessionOwner) return;

            isGameStarted = true;

            StartCoroutine(CountTime());

            MoveRandomPositionRpc();

            AssignRole();
        }

        private void OnGameEnd()
        {
            isGameStarted = false;

            UnassignRole();
        }

        private void AssignRole()
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            var seeker = Random.Range(0, clients.Count);

            for (var i = 0; i < clients.Count; i++)
            {
                if (seeker == i)
                {
                    seekerIds.Add(clients[i].ClientId);
                }
                else
                {
                    hiderIds.Add(clients[i].ClientId);
                }
            }
        }

        private void UnassignRole()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SetRoleRpc(PlayerEntity.Role.None,
                    RpcTarget.Single(client, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SetRoleRpc(PlayerEntity.Role role, RpcParams rpcParams)
        {
            var target = NetworkManager.Singleton
                .LocalClient.PlayerObject.GetComponent<PlayerEntity>();

            target.role.Value = role;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void HitRpc(RpcParams rpcParams)
        {
            var target = NetworkManager.Singleton
                .LocalClient.PlayerObject.GetComponent<PlayerEntity>();

            target.Damaged();
        }

        [Rpc(SendTo.Everyone)]
        private void MoveRandomPositionRpc()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            var randomPos = Util.GetRandomPositionInSphere(spawnRadius);

            var obj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            obj.transform.position = randomPos;
            obj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

            print($"Client {clientId}: Position = {randomPos}");
        }

        private IEnumerator CountTime()
        {
            print("Count Time Started");

            while (isGameStarted)
            {
                yield return waitDelay;

                currentTime.Value += 1;
            }
        }
    }
}