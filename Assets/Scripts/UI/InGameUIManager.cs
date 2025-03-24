using UnityEngine;

namespace UI
{
    public class InGameUIManager : MonoBehaviour
    {
        private void Update()
        {
            UIManager.HandleMouseLock();
        }
    }
}