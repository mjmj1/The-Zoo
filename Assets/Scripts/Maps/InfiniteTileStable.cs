using UnityEngine;

namespace Maps
{
    // Cinemachine가 카메라를 움직인 뒤 실행
    [DefaultExecutionOrder(10000)]
    public class InfiniteTileStable : MonoBehaviour
    {
        [SerializeField] Transform tileRoot;
        [SerializeField] float cellSize = 50f;

        [Header("따라갈 대상(필수)")]
        public Transform follow;

        [Header("카메라 기준 앞당김(선택)")]
        public Camera cam;
        [SerializeField] bool useCameraLookAhead = true;
        [SerializeField] float lookAhead = 8f;

        [Header("스냅 히스테리시스(여유)")]
        [SerializeField] float snapMargin = 2f;

        Vector2Int lastCell;
        bool inited;
        Vector3 prevWrappedAnchor;  // 이전 프레임 앵커(래핑 좌표)
        Vector3 virtualAnchor;      // 언랩된 연속 좌표
        TorusWorld world;

        void Awake() => world = TorusWorld.Instance ? TorusWorld.Instance : FindObjectOfType<TorusWorld>();

        void LateUpdate()
        {
            if (!tileRoot || !follow) return;
            if (!cam) cam = Camera.main;

            // 1) 앵커 계산: 카메라가 보는 방향으로 약간 앞당겨 빈칸 예방
            Vector3 anchor = follow.position;
            if (useCameraLookAhead && cam)
            {
                var flatFwd = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                anchor += flatFwd * lookAhead;
            }

            // 초기화
            if (!inited)
            {
                inited = true;
                prevWrappedAnchor = anchor;
                virtualAnchor     = anchor;
                lastCell = new Vector2Int(
                    Mathf.FloorToInt(virtualAnchor.x / cellSize),
                    Mathf.FloorToInt(virtualAnchor.z / cellSize));
                tileRoot.position = new Vector3(lastCell.x * cellSize, 0f, lastCell.y * cellSize);
                return;
            }

            // 2) 토러스 인지 델타(ShortestOffset)로 언랩 누적 => 연속 좌표
            float sx = world ? world.sizeX : cellSize * 3f;
            float sz = world ? world.sizeZ : cellSize * 3f;
            float halfX = sx * 0.5f, halfZ = sz * 0.5f;

            float dx = anchor.x - prevWrappedAnchor.x;
            float dz = anchor.z - prevWrappedAnchor.z;
            if (dx >  halfX) dx -= sx; else if (dx < -halfX) dx += sx;
            if (dz >  halfZ) dz -= sz; else if (dz < -halfZ) dz += sz;

            virtualAnchor += new Vector3(dx, 0f, dz);
            prevWrappedAnchor = anchor;

            // 3) 히스테리시스: 셀 중심에서 절반 - 여유를 넘을 때만 스냅
            Vector2Int targetCell = new(
                Mathf.FloorToInt(virtualAnchor.x / cellSize),
                Mathf.FloorToInt(virtualAnchor.z / cellSize));

            Vector3 snapped = new(lastCell.x * cellSize, 0f, lastCell.y * cellSize);
            float lx = Mathf.Abs(virtualAnchor.x - snapped.x);
            float lz = Mathf.Abs(virtualAnchor.z - snapped.z);
            float threshold = cellSize * 0.5f - snapMargin;

            if (lx > threshold || lz > threshold || targetCell != lastCell)
            {
                lastCell = targetCell;
                tileRoot.position = new Vector3(targetCell.x * cellSize, 0f, targetCell.y * cellSize);
            }
        }
    }
}
