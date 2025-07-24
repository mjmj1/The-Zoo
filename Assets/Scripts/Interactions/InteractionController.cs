using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Utils;
using System.Linq;

namespace Interactions
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private GameObject[] interactionObjects;

        private int[] selectedList;

        private void Start()
        {
            SpawnInteractionObjects(15);
        }

        private void SpawnInteractionObjects(int count)
        {
            //selectedList = Enumerable
            for (var i = 0; i < count; i++)
            {
                var randomNumber = Random.Range(0, count + 1);

                var spawnPoint = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());

                var surfaceUp = spawnPoint.normalized;

                var rotationOnSurface = Quaternion.FromToRotation(Vector3.up, surfaceUp);

                var randomYaw = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                var finalRotation = rotationOnSurface * randomYaw;

                Instantiate(interactionObjects[Random.Range(0, interactionObjects.Length)], spawnPoint, finalRotation);
            }
        }
    }
}