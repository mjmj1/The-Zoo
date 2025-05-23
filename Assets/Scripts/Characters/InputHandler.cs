using UnityEngine;

namespace Characters
{
    internal class InputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool InteractPressed { get; private set; }

        private PlayerInputActions _inputActions;
        
        private void Awake()
        {
            _inputActions = new PlayerInputActions();

            _inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

            _inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

            _inputActions.Player.Interact.performed += ctx => InteractPressed = true;
        }

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();

        private void LateUpdate()
        {
            InteractPressed = false; // Reset every frame
        }
    }
}