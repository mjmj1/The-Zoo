using System.Collections;
using Interactions;
using Maps;
using Mission;
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

        public NetworkVariable<bool> isGameStarted;
        public NetworkVariable<int> currentTime = new();
        private readonly WaitForSeconds waitDelay = new(1.0f);

        internal InteractionController interactor;
        internal ObserverManager observerManager;
        internal RoleManager roleManager;

        public static PlayManager Instance { get; private set; }

        public void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);
        }

        public void OnEnable()
        {
            gameResult.gameObject.SetActive(false);
        }

        public override void OnNetworkSpawn()
        {
            interactor = GetComponent<InteractionController>();
            observerManager = GetComponent<ObserverManager>();
            roleManager = GetComponent<RoleManager>();

            if (!IsOwner) return;

            currentTime.OnValueChanged += OnHiderTimeWinChecked;
            isGameStarted.OnValueChanged += OnGameStartedValueChanged;
            MissionManager.instance.missionGauge.HiderProgress.OnValueChanged +=
                OnHiderMissionWinChecked;

            roleManager.SeekerIds.OnListChanged += OnHiderWinChecked;
            roleManager.HiderIds.OnListChanged += OnSeekerWinChecked;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;

            currentTime.OnValueChanged -= OnHiderTimeWinChecked;
            isGameStarted.OnValueChanged -= OnGameStartedValueChanged;

            MissionManager.instance.missionGauge.HiderProgress.OnValueChanged -=
                OnHiderMissionWinChecked;

            roleManager.SeekerIds.OnListChanged -= OnHiderWinChecked;
            roleManager.HiderIds.OnListChanged -= OnSeekerWinChecked;
        }


        private void OnGameStartedValueChanged(bool previousValue, bool newValue)
        {
            if (!IsSessionOwner) return;

            if (newValue)
                StartCoroutine(GameStartRoutine());
            else
                roleManager.UnassignRole();
        }

        private void OnHiderWinChecked(NetworkListEvent<PlayerData> changeEvent)
        {
            if (!isGameStarted.Value) return;

            if (changeEvent.Type != NetworkListEvent<PlayerData>.EventType.Remove) return;

            if (roleManager.SeekerIds.Count != 0) return;

            isGameStarted.Value = false;

            ShowResultRpc(false);
        }

        private void OnHiderMissionWinChecked(int previousValue, int newValue)
        {
            if (!isGameStarted.Value) return;

            var totalMissionPoint =
                MissionManager.instance.MaxPickup + MissionManager.instance.MaxSpin;

            if (MissionManager.instance.missionGauge.HiderProgress.Value <
                totalMissionPoint) return;

            isGameStarted.Value = false;

            ShowResultRpc(false);
        }

        private void OnHiderTimeWinChecked(int previousValue, int newValue)
        {
            if (!isGameStarted.Value) return;

            if (newValue < 300) return;

            isGameStarted.Value = false;

            ShowResultRpc(false);
        }

        private void OnSeekerWinChecked(NetworkListEvent<PlayerData> changeEvent)
        {
            if (!isGameStarted.Value) return;

            if (changeEvent.Type != NetworkListEvent<PlayerData>.EventType.Remove) return;

            if (roleManager.HiderIds.Count != 0) return;

            isGameStarted.Value = false;

            ShowResultRpc(true);
        }

        protected override void OnInSceneObjectsSpawned()
        {
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

            var randomPos = Vector3.zero;

            if (PlanetGravity.Instance)
                randomPos = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());
            else if (TorusWorld.Instance)
                randomPos = Util.GetRandomPosition(-15f, 15f, -15f, 15f, 1f);

            var obj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            obj.transform.position = randomPos;
            obj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            obj.GetComponent<PlayerEntity>().AlignForward();
        }

        private IEnumerator GameStartRoutine()
        {
            MoveRandomPositionRpc();

            yield return null;

            roleManager.AssignRole();

            yield return null;

            // yield return StartCoroutine(SpawnNpc());

            HideLoadingRpc();

            StartCoroutine(CountTime());
        }

        private IEnumerator SpawnNpc()
        {
            yield return new WaitForSeconds(2f);

            foreach (var data in roleManager.HiderIds)
                NpcSpawner.Instance.SpawnNpcRpc(data.AnimalIndex, 5);
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