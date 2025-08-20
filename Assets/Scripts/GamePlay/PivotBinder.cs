using AmazingAssets.CurvedWorld;
using UnityEngine;

namespace GamePlay
{
    public class PivotBinder : MonoBehaviour
    {
        public static PivotBinder Instance { get; private set; }

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                controller = GetComponent<CurvedWorldController>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private CurvedWorldController controller;

        public void BindPivot(Transform pivot)
        {
            controller.bendPivotPoint = pivot;
        }
    }
}
