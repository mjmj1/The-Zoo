using UnityEngine;

public class InfinitePlaneVisual : MonoBehaviour
{
    [SerializeField] private Transform tileRoot;
    [SerializeField] private float cellSize = 30f;
    
    public Transform follow;
    Vector2Int lastCell;

    void LateUpdate()
    {
        if (!follow) return;
        var cell = new Vector2Int(
            Mathf.FloorToInt(follow.position.x / cellSize),
            Mathf.FloorToInt(follow.position.z / cellSize)
        );
        if (cell == lastCell) return;
        lastCell = cell;

        // 타일 묶음 중심 재배치
        tileRoot.position = new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);
    }
}