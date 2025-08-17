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

                Controller = GetComponent<CurvedWorldController>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        internal CurvedWorldController Controller;

        public void BindPivot(Transform pivot)
        {
            Controller.bendPivotPoint = pivot;
        }
    }
}
