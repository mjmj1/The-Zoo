using System.Collections;
using Networks;
using Players;
using UI;
using UI.GameResult;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace GamePlay
{
    public class PlayManager : NetworkBehaviour
    {
        [SerializeField] private GameResultUI gameResult;
        [SerializeField] private LoadingUI loading;
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

        public void OnEnable()
        {
            gameResult.gameObject.SetActive(false);
        }

        protected override void OnNetworkSessionSynchronized()
        {
            base.OnNetworkSessionSynchronized();

            MyLogger.Print(this, "OnNetworkSessionSynchronized");
        }

        public override void OnNetworkSpawn()
        {
            ObserverManager = GetComponent<ObserverManager>();
            RoleManager = GetComponent<RoleManager>();

            if (!IsOwner) return;

            currentTime.OnValueChanged += OnHiderWinChecked;
            isGameStarted.OnValueChanged += OnGameStartedValueChanged;
            ObserverManager.observerIds.OnListChanged += OnSeekerWinChecked;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;

            currentTime.OnValueChanged -= OnHiderWinChecked;
            isGameStarted.OnValueChanged -= OnGameStartedValueChanged;
            ObserverManager.observerIds.OnListChanged -= OnSeekerWinChecked;
        }


        private void OnGameStartedValueChanged(bool previousValue, bool newValue)
        {
            if (!IsSessionOwner) return;

            if (newValue)
            {
                StartCoroutine(GameStartRoutine());
            }
            else
            {
                RoleManager.UnassignRole();
            }
        }

        private void OnHiderWinChecked(int previousValue, int newValue)
        {
            if (!isGameStarted.Value) return;

            if (newValue < 300) return;

            isGameStarted.Value = false;

            ShowResultRpc(false);
        }

        private void OnSeekerWinChecked(NetworkListEvent<ulong> changeEvent)
        {
            if (!isGameStarted.Value) return;

            if (RoleManager.hiderIds.Count > ObserverManager.observerIds.Count) return;

            isGameStarted.Value = false;

            ShowResultRpc(true);
        }

        protected override void OnInSceneObjectsSpawned()
        {
            MyLogger.Print(this, "OnInSceneObjectsSpawned");

            if (!IsSessionOwner) return;

            base.OnInSceneObjectsSpawned();

            isGameStarted.Value = true;
        }

        [Rpc(SendTo.Everyone)]
        private void ShowResultRpc(bool isSeekerWin)
        {
            gameResult.OnGameResult(isSeekerWin);
            gameResult.SetButtonActive(IsSessionOwner);
            gameResult.gameObject.SetActive(true);
        }

        [Rpc(SendTo.Everyone)]
        private void HideLoadingRpc()
        {
            loading.gameObject.SetActive(false);
        }

        [Rpc(SendTo.Everyone)]
        private void MoveRandomPositionRpc()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            var randomPos = Util.GetRandomPositionInSphere(spawnRadius);

            var obj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            obj.transform.position = randomPos;
            obj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            obj.GetComponent<PlayerEntity>().AlignForward();
        }

        private IEnumerator GameStartRoutine()
        {
            MoveRandomPositionRpc();

            yield return StartCoroutine(SpawnNpc());

            HideLoadingRpc();

            StartCoroutine(CountTime());

            RoleManager.AssignRole();
        }

        private IEnumerator SpawnNpc()
        {
            yield return new WaitForSeconds(2f);

            NpcSpawner.Instance.SpawnNpcRpc(0, 5);
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
