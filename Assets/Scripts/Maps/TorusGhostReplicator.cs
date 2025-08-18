using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Maps
{
    /// <summary>
    /// 토러스 맵에서 경계 근처 오브젝트의 고스트를 효율적으로 관리합니다.
    /// 페이드 효과 없이 ON/OFF만 처리합니다.
    /// </summary>
    public class TorusGhostReplicator : NetworkBehaviour
    {
        [Header("설정")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float ghostBand = 8f;

        // 고스트 데이터 구조
        private struct GhostData
        {
            public GameObject gameObject;
            public bool isActive;
        }

        private static Vector3[] s_Offsets;
        private static bool s_OffsetsValid;
        
        private GhostData[] ghosts = new GhostData[8];
        private const int GHOST_COUNT = 8;

        public override void OnNetworkSpawn()
        {
            if (!IsClient) { enabled = false; return; }
            InitializeGhosts();
        }

        private void OnEnable() => InitializeGhosts();

        private void LateUpdate()
        {
            if (!IsClient || !IsSpawned || !visualRoot) return;
            
            var tw = TorusWorld.Instance;
            if (!tw) return;

            ValidateOffsets(tw);
            if (!s_OffsetsValid) return;

            UpdateGhosts(tw);
        }

        private void ValidateOffsets(TorusWorld world)
        {
            if (s_OffsetsValid && s_Offsets?.Length == GHOST_COUNT) return;

            s_Offsets = new Vector3[GHOST_COUNT]
            {
                new Vector3(+world.sizeX, 0, 0),        new Vector3(-world.sizeX, 0, 0),
                new Vector3(0, 0, +world.sizeZ),        new Vector3(0, 0, -world.sizeZ),
                new Vector3(+world.sizeX, 0, +world.sizeZ), new Vector3(+world.sizeX, 0, -world.sizeZ),
                new Vector3(-world.sizeX, 0, +world.sizeZ), new Vector3(-world.sizeX, 0, -world.sizeZ)
            };
            s_OffsetsValid = true;
        }

        private void UpdateGhosts(TorusWorld world)
        {
            var objectPos = world.WrapXZ(visualRoot.position);
            var baseTransform = (visualRoot.position, visualRoot.rotation, visualRoot.localScale);

            // 축별 활성화 여부 계산
            bool nearLeft = IsNearEdge(objectPos.x, -world.HalfX, true);
            bool nearRight = IsNearEdge(objectPos.x, world.HalfX, false);
            bool nearBottom = IsNearEdge(objectPos.z, -world.HalfZ, true);
            bool nearTop = IsNearEdge(objectPos.z, world.HalfZ, false);

            // 직교 고스트 (4방향)
            SetGhost(0, nearLeft, baseTransform, s_Offsets[0]);
            SetGhost(1, nearRight, baseTransform, s_Offsets[1]);
            SetGhost(2, nearBottom, baseTransform, s_Offsets[2]);
            SetGhost(3, nearTop, baseTransform, s_Offsets[3]);

            // 대각선 고스트 (4방향)
            SetGhost(4, nearLeft && nearBottom, baseTransform, s_Offsets[4]);
            SetGhost(5, nearLeft && nearTop, baseTransform, s_Offsets[5]);
            SetGhost(6, nearRight && nearBottom, baseTransform, s_Offsets[6]);
            SetGhost(7, nearRight && nearTop, baseTransform, s_Offsets[7]);
        }

        private bool IsNearEdge(float coordinate, float boundary, bool isNegative)
        {
            float distance = isNegative ? coordinate - boundary : boundary - coordinate;
            return distance <= ghostBand;
        }

        private void SetGhost(int index, bool shouldBeActive, (Vector3 pos, Quaternion rot, Vector3 scale) baseTransform, Vector3 offset)
        {
            ref var ghost = ref ghosts[index];

            if (!shouldBeActive)
            {
                if (ghost.isActive)
                {
                    ghost.gameObject.SetActive(false);
                    ghost.isActive = false;
                }
                return;
            }

            if (!ghost.isActive)
            {
                ghost.gameObject.SetActive(true);
                ghost.isActive = true;
            }

            // Transform 업데이트
            var transform = ghost.gameObject.transform;
            transform.SetPositionAndRotation(baseTransform.pos + offset, baseTransform.rot);
            transform.localScale = baseTransform.scale;
        }

        private void InitializeGhosts()
        {
            if (!visualRoot) return;

            DestroyExistingGhosts();

            for (int i = 0; i < GHOST_COUNT; i++)
            {
                var ghostObject = CreateGhostObject(i);
                
                ghosts[i] = new GhostData
                {
                    gameObject = ghostObject,
                    isActive = false
                };
            }
        }

        private GameObject CreateGhostObject(int index)
        {
            var ghost = Instantiate(visualRoot.gameObject, transform);
            ghost.name = $"Ghost_{index}";
            
            // 물리/충돌 제거
            RemovePhysicsComponents(ghost);
            
            ghost.SetActive(false);
            return ghost;
        }

        private void RemovePhysicsComponents(GameObject ghost)
        {
            // 컴포넌트 비활성화/제거를 한 번에 처리
            var colliders = ghost.GetComponentsInChildren<Collider>(true);
            var rigidbodies = ghost.GetComponentsInChildren<Rigidbody>(true);
            var replicators = ghost.GetComponentsInChildren<TorusGhostReplicator>(true);
            var networkObjects = ghost.GetComponentsInChildren<NetworkObject>(true);

            foreach (var col in colliders) col.enabled = false;
            foreach (var rb in rigidbodies) rb.isKinematic = true;
            foreach (var rep in replicators) if (rep != this) Destroy(rep);
            foreach (var no in networkObjects) Destroy(no);
        }

        private void DestroyExistingGhosts()
        {
            for (int i = 0; i < ghosts.Length; i++)
            {
                if (ghosts[i].gameObject) 
                {
                    Destroy(ghosts[i].gameObject);
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ghostBand = Mathf.Max(0f, ghostBand);
        }
#endif
    }
}