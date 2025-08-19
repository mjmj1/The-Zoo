using Unity.Netcode;
using UnityEngine;

namespace Maps
{
    /// <summary>
    ///     토러스 맵에서 경계 근처 오브젝트의 고스트를 효율적으로 관리합니다.
    ///     페이드 효과 없이 ON/OFF만 처리합니다.
    /// </summary>
    public class TorusGhostReplicator : NetworkBehaviour
    {
        private const int GhostCount = 8;

        private static Vector3[] sOffsets;
        private static bool sOffsetsValid;

        [SerializeField] private Transform visualRoot;
        [SerializeField] private float ghostBand = 8f;

        private GhostData[] ghosts = new GhostData[8];

        private void LateUpdate()
        {
            if (!IsClient || !IsSpawned || !visualRoot) return;

            var tw = TorusWorld.Instance;
            if (!tw) return;

            ValidateOffsets(tw);
            if (!sOffsetsValid) return;

            UpdateGhosts(tw);
        }

        private void OnEnable()
        {
            InitializeGhosts();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ghostBand = Mathf.Max(0f, ghostBand);
        }
#endif

        public override void OnNetworkSpawn()
        {
            if (!IsClient)
            {
                enabled = false;
                return;
            }

            InitializeGhosts();
        }

        private void ValidateOffsets(TorusWorld world)
        {
            if (sOffsetsValid && sOffsets?.Length == GhostCount) return;

            sOffsets = new Vector3[GhostCount]
            {
                new(+world.sizeX, 0, 0), new(-world.sizeX, 0, 0),
                new(0, 0, +world.sizeZ), new(0, 0, -world.sizeZ),
                new(+world.sizeX, 0, +world.sizeZ), new(+world.sizeX, 0, -world.sizeZ),
                new(-world.sizeX, 0, +world.sizeZ), new(-world.sizeX, 0, -world.sizeZ)
            };
            sOffsetsValid = true;
        }

        private void UpdateGhosts(TorusWorld world)
        {
            var objectPos = world.WrapXZ(visualRoot.position);
            var baseTransform = (visualRoot.position, visualRoot.rotation, visualRoot.localScale);

            var nearLeft = IsNearEdge(objectPos.x, -world.HalfX, true);
            var nearRight = IsNearEdge(objectPos.x, world.HalfX, false);
            var nearBottom = IsNearEdge(objectPos.z, -world.HalfZ, true);
            var nearTop = IsNearEdge(objectPos.z, world.HalfZ, false);

            SetGhost(0, nearLeft, baseTransform, sOffsets[0]);
            SetGhost(1, nearRight, baseTransform, sOffsets[1]);
            SetGhost(2, nearBottom, baseTransform, sOffsets[2]);
            SetGhost(3, nearTop, baseTransform, sOffsets[3]);

            SetGhost(4, nearLeft && nearBottom, baseTransform, sOffsets[4]);
            SetGhost(5, nearLeft && nearTop, baseTransform, sOffsets[5]);
            SetGhost(6, nearRight && nearBottom, baseTransform, sOffsets[6]);
            SetGhost(7, nearRight && nearTop, baseTransform, sOffsets[7]);
        }

        private bool IsNearEdge(float coordinate, float boundary, bool isNegative)
        {
            var distance = isNegative ? coordinate - boundary : boundary - coordinate;
            return distance <= ghostBand;
        }

        private void SetGhost(int index, bool shouldBeActive,
            (Vector3 pos, Quaternion rot, Vector3 scale) baseTransform, Vector3 offset)
        {
            ref var ghost = ref ghosts[index];

            if (!shouldBeActive)
            {
                if (ghost.IsActive)
                {
                    ghost.GameObject.SetActive(false);
                    ghost.IsActive = false;
                }

                return;
            }

            if (!ghost.IsActive)
            {
                ghost.GameObject.SetActive(true);
                ghost.IsActive = true;
            }

            var tr = ghost.GameObject.transform;
            tr.SetPositionAndRotation(baseTransform.pos + offset, baseTransform.rot);
            tr.localScale = baseTransform.scale;
        }

        private void InitializeGhosts()
        {
            if (!visualRoot) return;

            DestroyExistingGhosts();

            for (var i = 0; i < GhostCount; i++)
            {
                var ghostObject = CreateGhostObject(i);

                ghosts[i] = new GhostData
                {
                    GameObject = ghostObject,
                    IsActive = false
                };
            }
        }

        private GameObject CreateGhostObject(int index)
        {
            var ghost = Instantiate(visualRoot.gameObject, transform);
            ghost.name = $"Ghost_{index}";

            RemovePhysicsComponents(ghost);

            ghost.SetActive(false);
            return ghost;
        }

        private void RemovePhysicsComponents(GameObject ghost)
        {
            var colliders = ghost.GetComponentsInChildren<Collider>(true);
            var rigidbodies = ghost.GetComponentsInChildren<Rigidbody>(true);
            var replicators = ghost.GetComponentsInChildren<TorusGhostReplicator>(true);
            var networkObjects = ghost.GetComponentsInChildren<NetworkObject>(true);

            foreach (var col in colliders) col.enabled = false;
            foreach (var rb in rigidbodies) rb.isKinematic = true;
            foreach (var rep in replicators)
                if (rep != this)
                    Destroy(rep);
            foreach (var no in networkObjects) Destroy(no);
        }

        private void DestroyExistingGhosts()
        {
            for (var i = 0; i < ghosts.Length; i++)
                if (ghosts[i].GameObject)
                    Destroy(ghosts[i].GameObject);
        }

        // 고스트 데이터 구조
        private struct GhostData
        {
            public GameObject GameObject;
            public bool IsActive;
        }
    }
}