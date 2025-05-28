using UnityEngine;

namespace Characters
{
    internal class InputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool SprintPressed { get; private set; }

        public PlayerInputActions InputActions { get; private set; }
        
        private void Awake()
        {
            InputActions = new PlayerInputActions();

            InputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            InputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

            InputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
            InputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

            InputActions.Player.Interact.performed += ctx => InteractPressed = true;
            
            InputActions.Player.Sprint.performed += ctx => SprintPressed = true;
            InputActions.Player.Sprint.canceled += ctx => SprintPressed = false;
        }

        private void OnEnable() => InputActions.Enable();
        private void OnDisable() => InputActions.Disable();

        private void LateUpdate()
        {
            InteractPressed = false; // Reset every frame
        }
    }
}