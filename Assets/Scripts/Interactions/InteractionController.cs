using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Interactions
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private GameObject[] interactionObjects;
        [SerializeField] private int InteractionsNumber = 15;

        private List<int> RandomNumberList = new List<int>();
        private int targetCount = 5;

        private void Start()
        {
            SpawnInteractionObjects(InteractionsNumber);
        }

        private void SpawnInteractionObjects(int count)
        {
            List<int> allIndexes = new List<int>();
            for (int i = 0; i < count; i++)
                allIndexes.Add(i);

            for (int i = 0; i < targetCount; i++)
            {
                int rand = Random.Range(0, allIndexes.Count);
                RandomNumberList.Add(allIndexes[rand]);
                allIndexes.RemoveAt(rand);
            }

            for (var i = 0; i < count; i++)
            {
                var spawnPoint = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());

                var surfaceUp = spawnPoint.normalized;

                var rotationOnSurface = Quaternion.FromToRotation(Vector3.up, surfaceUp);

                var randomYaw = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                var finalRotation = rotationOnSurface * randomYaw;

                var obj = Instantiate(interactionObjects[Random.Range(0, interactionObjects.Length)], spawnPoint, finalRotation);

                var targetMission = RandomNumberList.Contains(i);
                MyLogger.Print(this, "targetMission : " + targetMission);
                obj.GetComponent<InteractableSpawner>().Initailize(targetMission);
            }
        }
    }
}