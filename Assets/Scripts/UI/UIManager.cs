using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject preferencesWindow;

        public void OnPreferencesClick()
        {
            preferencesWindow.SetActive(!preferencesWindow.activeSelf);
        }
    }
}
