using UnityEngine;

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
            var points = new Vector3[numberOfObjects];

            for (var i = 0; i < numberOfObjects; i++)
            {
                points[i] = Random.onUnitSphere.normalized;
            }

            foreach (var point in points)
            {
                var spawnPosition = _planetCollider.transform.position + point * _planetCollider.radius * planet.transform.localScale.x;
                var normal = (spawnPosition - _planetCollider.transform.position).normalized;

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

                Instantiate(interactionObjects, spawnPosition, rotation);
            }
        }
    }
}