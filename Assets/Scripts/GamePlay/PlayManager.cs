using System.Collections;
using Players;
using UI.GameResult;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        [SerializeField] private GameResultUI gameResult;
        [SerializeField] private float spawnRadius = 7.5f;

        public NetworkVariable<bool> isGameStarted;
        public NetworkVariable<int> currentTime = new();
        private readonly WaitForSeconds waitDelay = new(1.0f);

        internal ObserverManager ObserverManager;
        internal RoleManager RoleManager;


        public static PlayManager Instance { get; private set; }

        public void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            ObserverManager = GetComponent<ObserverManager>();
            RoleManager = GetComponent<RoleManager>();

            if (!IsOwner) return;

            currentTime.OnValueChanged += OnValueChanged;
            isGameStarted.OnValueChanged += OnGameStartedValueChanged;
            ObserverManager.observerIds.OnListChanged += OnListChanged;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;

            currentTime.OnValueChanged -= OnValueChanged;
            isGameStarted.OnValueChanged -= OnGameStartedValueChanged;
            ObserverManager.observerIds.OnListChanged -= OnListChanged;
        }

        private void OnGameStartedValueChanged(bool previousValue, bool newValue)
        {
            if (!IsSessionOwner) return;

            if (newValue)
            {
                StartCoroutine(CountTime());

                MoveRandomPositionRpc();

                RoleManager.AssignRole();
            }
            else
            {
                RoleManager.UnassignRole();
            }
        }

        private void OnValueChanged(int previousValue, int newValue)
        {
            if (newValue >= 20)
            {
                HiderWin();
            }
        }

        private void OnListChanged(NetworkListEvent<ulong> changeEvent)
        {
            if (RoleManager.hiderIds.Count <= ObserverManager.observerIds.Count)
            {
                SeekerWin();
            }
        }

        protected override void OnInSceneObjectsSpawned()
        {
            if (!IsSessionOwner) return;

            base.OnInSceneObjectsSpawned();

            isGameStarted.Value = true;
        }

        private void SeekerWin()
        {
            if (!isGameStarted.Value) return;

            isGameStarted.Value = false;

            ShowResultRpc(true);
        }

        private void HiderWin()
        {
            if (!isGameStarted.Value) return;

            isGameStarted.Value = false;

            ShowResultRpc(false);
        }

        [Rpc(SendTo.Everyone)]
        private void ShowResultRpc(bool isSeekerWin)
        {
            gameResult.OnGameResult(isSeekerWin);
            gameResult.gameObject.SetActive(true);
        }

        [Rpc(SendTo.Everyone)]
        private void MoveRandomPositionRpc()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            var randomPos = Util.GetRandomPositionInSphere(spawnRadius);

            var obj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            obj.transform.position = randomPos;
            obj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }

        private IEnumerator CountTime()
        {
            while (isGameStarted.Value)
            {
                yield return waitDelay;

                currentTime.Value += 1;
            }
        }
    }
}