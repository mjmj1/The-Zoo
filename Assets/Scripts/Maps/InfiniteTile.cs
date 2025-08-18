using UnityEngine;

namespace Maps
{
    public class InfiniteTile : MonoBehaviour
    {
        [SerializeField] private Transform tileRoot;
        [SerializeField] private float cellSize = 30f;

        public Transform follow;
        private Vector2Int lastCell;

        private void LateUpdate()
        {
            if (!follow) return;
            var cell = new Vector2Int(
                Mathf.FloorToInt(follow.position.x / cellSize),
                Mathf.FloorToInt(follow.position.z / cellSize)
            );
            if (cell == lastCell) return;
            lastCell = cell;

            tileRoot.position = new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);
        }
    }
}