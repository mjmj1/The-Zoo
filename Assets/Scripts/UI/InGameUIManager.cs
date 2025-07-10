using TMPro;
using UnityEngine;

namespace UI
{
    public class InGameUIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI interactionText;
        [SerializeField] private GameObject interactionPanel;

        public void ShowInteractionPrompt(string text)
        {
            interactionText.text = text;
            interactionPanel.SetActive(true);
        }

        public void HideInteractionPrompt()
        {
            interactionPanel.SetActive(false);
        }
    }
}