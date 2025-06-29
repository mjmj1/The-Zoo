using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utils;

namespace Characters
{
    internal class InputHandler : MonoBehaviour
    {
        private bool _attackPressed;
        private bool _isOverUI;
        private bool _spinPressed;
        private bool _sprintPressed;
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        public bool SpinPressed
        {
            get => _spinPressed;
            private set
            {
                _spinPressed = value;
                OnSpinPressed?.Invoke(value);
            }
        }

        public bool SprintPressed
        {
            get => _sprintPressed;
            private set
            {
                _sprintPressed = value;
                OnSprintPressed?.Invoke(value);
            }
        }

        public bool AttackPressed
        {
            get => _attackPressed;
            private set
            {
                _attackPressed = value;
                OnAttackPressed?.Invoke(value);
            }
        }

        public PlayerInputActions InputActions { get; private set; }

        private void Awake()
        {
            InputActions = new PlayerInputActions();

            InputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            InputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

            InputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
            InputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

            InputActions.Player.Attack.performed += ctx => AttackPressed = true;
            InputActions.Player.Attack.canceled += ctx => AttackPressed = false;

            InputActions.Player.Sprint.performed += ctx => SprintPressed = true;
            InputActions.Player.Sprint.canceled += ctx => SprintPressed = false;

            InputActions.Player.Spin.performed += ctx => SpinPressed = true;
            InputActions.Player.Spin.canceled += ctx => SpinPressed = false;

            InputActions.UI.Escape.performed += EscapePressed;
            InputActions.UI.Click.performed += MouseLeftClicked;
        }

        private void Update()
        {
            if(!InputActions.Player.enabled)
                _isOverUI = IsPointerOverUI();
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
            if (_isOverUI) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            InputActions.Player.Enable();
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
        }

        public event Action<bool> OnAttackPressed;
        public event Action<bool> OnSprintPressed;
        public event Action<bool> OnSpinPressed;
    }
}