using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class QuitButton : Button
    {
        private void OnClick()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}