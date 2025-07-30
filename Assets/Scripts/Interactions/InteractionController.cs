using EventHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Utils;
using static UnityEditor.PlayerSettings;

namespace Interactions
{
    public class InteractionController : NetworkBehaviour
    {
        public static InteractionController instance;
        [SerializeField] private GameObject[] interactionObjects;
        [SerializeField] private int InteractionsNumber = 15;

        private List<int> RandomNumberList = new List<int>();
        public int TargetCount = 5;

        // sycn
        [SerializeField] private List<NetworkObject> interactionPrefabs;

        private List<NetworkObject> spawnedInteractions = new();

        private void Awake()
        {
            if(instance == null)
                instance = this;
        }
        private void Start()
        {
            SpawnInteractionObjectsRpc(0, InteractionsNumber);
        }

        [Rpc(SendTo.Server, RequireOwnership = true)]
        private void SpawnInteractionObjectsRpc(int index, int count, RpcParams rpcParams = default)
        {
            List<int> allIndexes = new List<int>();
            for (int i = 0; i < count; i++)
                allIndexes.Add(i);

            for (int i = 0; i < TargetCount; i++)
            {
                int rand = UnityEngine.Random.Range(0, allIndexes.Count);
                RandomNumberList.Add(allIndexes[rand]);
                allIndexes.RemoveAt(rand);
            }

            var prefab = interactionPrefabs[index];

            for (var i = 0; i < count; i++)
            {
                var spawnPoint = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());

                var surfaceUp = spawnPoint.normalized;

                var rotationOnSurface = Quaternion.FromToRotation(Vector3.up, surfaceUp);

                var randomYaw = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);

                var finalRotation = rotationOnSurface * randomYaw;

                //var obj = Instantiate(interactionObjects[UnityEngine.Random.Range(0, interactionObjects.Length)], spawnPoint, finalRotation);
                //obj.GetComponent<InteractableSpawner>().Initailize(targetMission);

                var targetMission = RandomNumberList.Contains(i);
                
                var interaction = prefab.InstantiateAndSpawn(NetworkManager,
                    position: spawnPoint,
                    rotation: finalRotation);
                spawnedInteractions.Add(interaction);

                interaction.GetComponent<InteractableSpawner>().Initailize(targetMission);
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnInteractionRpc(RpcParams rpcParams = default)
        {
            foreach (var obj in spawnedInteractions)
            {
                obj.Despawn();
            }
        }
    }
}