using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Maps
{
    public class TorusGhostReplicator : NetworkBehaviour
    {
        public static Vector3[] _offs;

        [Header("렌더 전용 루트")]
        [SerializeField] private Transform visualRoot;

        [Header("경계 근접 밴드(m)")]
        [SerializeField] private float ghostBand = 2f;

        private readonly List<GameObject> ghosts = new();

        void Awake()
        {
            if (_offs == null && TorusWorld.Instance != null) BuildOffsets();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsClient) { enabled = false; return; }

            if (_offs == null || _offs.Length != 8)
                BuildOffsets();

            if (ghosts.Count == 0)
                CreateGhostPool();
        }

        void BuildOffsets()
        {
            var sx = TorusWorld.Instance.sizeX;
            var sz = TorusWorld.Instance.sizeZ;
            _offs = new[]
            {
                new Vector3(+sx,0,0), new Vector3(-sx,0,0),
                new Vector3(0,0,+sz), new Vector3(0,0,-sz),
                new Vector3(+sx,0,+sz), new Vector3(+sx,0,-sz),
                new Vector3(-sx,0,+sz), new Vector3(-sx,0,-sz),
            };
        }

        private void CreateGhostPool()
        {
            if (!visualRoot) return;

            for (var i = 0; i < 8; i++)
            {
                var g = Instantiate(visualRoot.gameObject, transform);
                g.name = $"Ghost_{i}";
                foreach (var c in g.GetComponentsInChildren<Collider>(true)) c.enabled = false;
                foreach (var rb in g.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
                g.SetActive(false);
                ghosts.Add(g);
            }
        }

        private void LateUpdate()
        {
            // 네트워크/초기화 보장
            if (!IsClient || !IsSpawned) return;
            if (!visualRoot) return;

            if (_offs == null || _offs.Length != 8)
            {
                BuildOffsets();
                if (_offs == null) return; // 여전히 없으면 다음 프레임에 재시도
            }
            if (ghosts.Count < 8) { CreateGhostPool(); if (ghosts.Count < 8) return; }

            var pos = transform.position;
            var sx = TorusWorld.Instance.sizeX;
            var sz = TorusWorld.Instance.sizeZ;

            bool nearLeft   = pos.x < -TorusWorld.Instance.HalfX + ghostBand;
            bool nearRight  = pos.x >  TorusWorld.Instance.HalfX - ghostBand;
            bool nearBottom = pos.z < -TorusWorld.Instance.HalfZ + ghostBand;
            bool nearTop    = pos.z >  TorusWorld.Instance.HalfZ - ghostBand;

            SetGhost(0, nearLeft,   _offs[0]);
            SetGhost(1, nearRight,  _offs[1]);
            SetGhost(2, nearBottom, _offs[2]);
            SetGhost(3, nearTop,    _offs[3]);
            SetGhost(4, nearLeft  && nearBottom, _offs[4]);
            SetGhost(5, nearLeft  && nearTop,    _offs[5]);
            SetGhost(6, nearRight && nearBottom, _offs[6]);
            SetGhost(7, nearRight && nearTop,    _offs[7]);
        }

        private void SetGhost(int idx, bool active, Vector3 off)
        {
            var g = ghosts[idx];
            if (!active) { if (g.activeSelf) g.SetActive(false); return; }
            if (!g.activeSelf) g.SetActive(true);

            g.transform.SetPositionAndRotation(visualRoot.position + off, visualRoot.rotation);
            g.transform.localScale = visualRoot.localScale;
        }
    }
}
