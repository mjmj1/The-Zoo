using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Players
{
    internal class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        public PlayerInputActions InputActions { get; private set; }

        private void Awake()
        {
            InputActions = new PlayerInputActions();

            InputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            InputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

            InputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
            InputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

            InputActions.UI.Escape.performed += EscapePressed;
            InputActions.UI.Click.performed += MouseLeftClicked;

            InputActions.UI.Alt.performed += AltButtonPressed;
            InputActions.UI.Alt.canceled += AltButtonPressed;
        }

        private void OnEnable()
        {
            InputActions.Enable();
        }

        private void OnDisable()
        {
            InputActions.Disable();
        }

        private void OnDestroy()
        {
            InputActions.UI.Escape.performed -= EscapePressed;
            InputActions.UI.Click.performed -= MouseLeftClicked;

            InputActions.UI.Alt.performed -= AltButtonPressed;
            InputActions.UI.Alt.canceled -= AltButtonPressed;
        }

        private void AltButtonPressed(InputAction.CallbackContext ctx)
        {
            if(ctx.performed)
                ShowCursor();
            else
                HideCursor();
        }

        private void EscapePressed(InputAction.CallbackContext ctx)
        {
            ShowCursor();
        }

        private void MouseLeftClicked(InputAction.CallbackContext ctx)
        {
            if (IsPointerOverUI()) return;

            HideCursor();
        }

        internal void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            CameraManager.Instance.EnableCamera(true);
            InputActions.Player.Enable();
        }

        internal void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            CameraManager.Instance.EnableCamera(false);
            InputActions.Player.Disable();
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
        }
    }
}