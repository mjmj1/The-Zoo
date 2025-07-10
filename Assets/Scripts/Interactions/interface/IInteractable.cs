using UnityEngine;
using UnityEngine.UI;

namespace Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        public enum InteractableType
        {
            LeftClick,
            RightClick,
            R,
            F
        }

        [SerializeField] protected Image interactionUI;
        [SerializeField] private Canvas canvas;

        private bool isFocused = false;

        private void Start()
        {
            if (interactionUI != null)
            {
                interactionUI.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            interactionUI.gameObject.SetActive(true);

            var screenPos = Camera.main.WorldToScreenPoint(interactionUI.transform.position);

            // 2) 스크린 좌표 → 캔버스 로컬 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                null,
                out var localPoint
            );

            interactionUI.transform.position = localPoint;
        }

        private void OnDisable()
        {

        }

        public void ShowInteractableUI()
        {
            interactionUI?.gameObject.SetActive(true);
        }

        public void HideInteractableUI()
        {
            interactionUI?.gameObject.SetActive(false);
        }

        public abstract void StartInteract();
        public abstract void StopInteract();

        public abstract InteractableType GetInteractableType();
    }
}