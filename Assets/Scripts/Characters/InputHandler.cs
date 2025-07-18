using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Characters
{
    internal class InputHandler : MonoBehaviour
    {
        private bool isOverUI;

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
        }

        private void Update()
        {
            if (!InputActions.Player.enabled)
                isOverUI = IsPointerOverUI();
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
        }

        private void EscapePressed(InputAction.CallbackContext ctx)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            InputActions.Player.Disable();
        }

        private void MouseLeftClicked(InputAction.CallbackContext ctx)
        {
            if (isOverUI) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            InputActions.Player.Enable();
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
        }
    }
}