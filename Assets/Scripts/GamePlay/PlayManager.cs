using System.Collections;
using Players;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        [SerializeField] private GameObject observerPrefab;
        [SerializeField] private float spawnRadius = 7.5f;

        public NetworkVariable<int> currentTime = new();
        private readonly WaitForSeconds waitDelay = new(1.0f);

        internal ObserverManager ObserverManager;
        internal RoleManager RoleManager;

        private bool isGameStarted;

        public static PlayManager Instance { get; private set; }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                ObserverManager = GetComponent<ObserverManager>();
                RoleManager = GetComponent<RoleManager>();
            }
            else Destroy(gameObject);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            OnGameEnd();
        }

        protected override void OnInSceneObjectsSpawned()
        {
            base.OnInSceneObjectsSpawned();

            if (!IsSessionOwner) return;

            OnGameStart();
        }

        private void OnGameStart()
        {
            if (!IsSessionOwner) return;

            isGameStarted = true;

            StartCoroutine(CountTime());

            MoveRandomPositionRpc();

            RoleManager.AssignRole();
        }

        private void OnGameEnd()
        {
            isGameStarted = false;

            RoleManager.UnassignRole();
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