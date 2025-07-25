using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Interactions
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private GameObject[] interactionObjects;
        [SerializeField] private int InteractionsNumber = 15;
        [SerializeField] private TMP_Text treeCountText;
        [SerializeField] private TMP_Text playerCountText;

        private List<int> RandomNumberList = new List<int>();
        public int TargetCount = 5;

        private void Start()
        {
            //treeCountText.text = " : " + TargetCount.ToString();

            //playerCountText.text = " : " + GameManager.Instance.PlayerCount.ToString();

            SpawnInteractionObjects(InteractionsNumber);
        }

        private void SpawnInteractionObjects(int count)
        {
            List<int> allIndexes = new List<int>();
            for (int i = 0; i < count; i++)
                allIndexes.Add(i);

            for (int i = 0; i < TargetCount; i++)
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
                
                obj.GetComponent<InteractableSpawner>().Initailize(targetMission);
            }
        }
    }
}