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

                currentTime.OnValueChanged += OnValueChanged;
                ObserverManager.observerIds.OnListChanged += OnListChanged;
            }
            else Destroy(gameObject);
        }

        private void OnValueChanged(int previousValue, int newValue)
        {
            if (newValue >= 300)
            {
                if (RoleManager.hiderIds.Count <= ObserverManager.observerIds.Count) return;

                HiderWin();
            }
        }

        private void OnListChanged(NetworkListEvent<ulong> changeEvent)
        {
            if (RoleManager.hiderIds.Count >= ObserverManager.observerIds.Count)
            {
                SeekerWin();
            }
        }

        public override void OnDestroy()
        {
            MyLogger.Print(this, "OnDestroy");
            OnGameEnd();

            base.OnDestroy();
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

        private void SeekerWin()
        {
            OnGameEnd();

            if (!isGameStarted) return;

            gameResult.SetTitle("Seeker Win !");
            gameResult.gameObject.SetActive(true);
            gameResult.OnGameResult(true);
        }

        private void HiderWin()
        {
            OnGameEnd();
            
            if (!isGameStarted) return;

            gameResult.SetTitle("Hider Win !");
            gameResult.gameObject.SetActive(true);
            gameResult.OnGameResult(false);
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