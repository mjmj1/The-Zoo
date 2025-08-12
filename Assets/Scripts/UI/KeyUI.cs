using Scriptable;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class KeyUI : MonoBehaviour
    {
        [SerializeField] private KeyUIData keyUiData;

        private Image background;

        private void Awake()
        {
            background = GetComponent<Image>();
        }

        internal void Interactable()
        {
            SetColor(keyUiData.interactableColor);
        }

        internal void NonInteractable()
        {
            SetColor(keyUiData.nonInteractableColor);
        }

        internal void Unable()
        {
            SetColor(keyUiData.unableColor);
        }

        private void SetColor(Color color)
        {
            background.color = color;
        }
    }
}
