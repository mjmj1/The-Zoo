using UnityEngine;

namespace Interactions
{
    public class InteractableSpawner : Interactable
    {
        [SerializeField] private GameObject spawnObject;
        [SerializeField] private BoxCollider[] spawnPoints;
        [SerializeField] private Vector2 downForceRange = new (0f, 1f);
        
        private bool isInteracting = false;
        
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (spawnPoints == null) return;
            
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Gizmos.color = Color.yellow;

            Vector3 halfSize = spawnPoint.size * 0.5f;
            Vector3 center   = spawnPoint.center;

            // 로컬 그리기: 8개 모서리 구하기
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 localCorner = center + Vector3.Scale(halfSize, new Vector3(x,y,z));
                Vector3 worldCorner = spawnPoint.transform.TransformPoint(localCorner);
                Gizmos.DrawSphere(worldCorner, 0.05f);
            }
        }
#endif
        
        public override void StartInteract()
        {
            if (isInteracting) return;
            
            isInteracting = true;

            Spawn();
            
            print($"{gameObject.name} is interacting...");
        }

        public override void StopInteract()
        {
            if (!isInteracting) return;
            
            isInteracting = false;
            print($"{gameObject.name} is stop interacting...");
        }

        private void Spawn()
        {
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            var min = spawnPoint.bounds.min;
            var max = spawnPoint.bounds.max;
            var spawnPos = new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
            );

            var fruit = Instantiate(spawnObject, spawnPos, Quaternion.identity, spawnPoint.transform);
            
            var rb = fruit.GetComponent<Rigidbody>();
            
            if (rb == null)
            {
                var force = Random.Range(downForceRange.x, downForceRange.y);
                rb.AddForce(Vector3.down * force, ForceMode.Impulse);
            }
        }
        
        public override InteractableType GetInteractableType()
        {
            return InteractableType.LeftClick;
        }
    }
}