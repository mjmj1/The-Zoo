using UnityEngine;
using Utils;

namespace Interactions
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private GameObject planet;
        [SerializeField] private GameObject interactionObjects;
    
        private SphereCollider _planetCollider;

        private void Start()
        {
            _planetCollider = planet.GetComponent<SphereCollider>();

            GetRandomPositionForInteractionObjects(10);
        }

        /// <summary>
        /// A function that creates interaction objects in random position
        /// </summary>
        /// <param name="numberOfObjects">number of interaction objects</param>
        private void GetRandomPositionForInteractionObjects(int numberOfObjects)
        {
            for (var i = 0; i < numberOfObjects; i++)
            {
                var spawnPosition = _planetCollider.transform.position + 
                                    Util.GetRandomPositionInSphere(_planetCollider.radius)
                                    * planet.transform.localScale.x;
                var normal = (spawnPosition - _planetCollider.transform.position).normalized;

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

                Instantiate(interactionObjects, spawnPosition, rotation);
            }
        }
    }
}