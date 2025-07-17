using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    public class CharacterNetworkAnimator : NetworkAnimator
    {
        public static readonly int MoveHash = Animator.StringToHash("Walk");
        public static readonly int StopMoveHash = Animator.StringToHash("StopWalk");
        public static readonly int RunHash = Animator.StringToHash("Run");
        public static readonly int StopRunHash = Animator.StringToHash("StopRun");
        public static readonly int JumpHash = Animator.StringToHash("Jump");
        public static readonly int LandHash = Animator.StringToHash("Land");
        public static readonly int SpinHash = Animator.StringToHash("Spin");
        public static readonly int StopSpinHash = Animator.StringToHash("StopSpin");
        public static readonly int AttackHash = Animator.StringToHash("Attack");
        public static readonly int StopAttackHash = Animator.StringToHash("StopAttack");

        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }

        internal void Initialize(InputHandler input)
        {
            input.InputActions.Player.Move.performed += OnMove;
            input.InputActions.Player.Move.canceled += OnMove;
            input.InputActions.Player.Run.performed += OnRun;
            input.InputActions.Player.Run.canceled += OnRun;
            input.InputActions.Player.Jump.performed += OnJump;
            input.InputActions.Player.Attack.performed += OnAttack;
            input.InputActions.Player.Attack.canceled += OnAttack;
            input.InputActions.Player.Spin.performed += OnSpin;
            input.InputActions.Player.Spin.canceled += OnSpin;

        }

        internal void OnDestroying(InputHandler input)
        {
            input.InputActions.Player.Move.performed -= OnMove;
            input.InputActions.Player.Move.canceled -= OnMove;
            input.InputActions.Player.Run.performed -= OnRun;
            input.InputActions.Player.Run.canceled -= OnRun;
            input.InputActions.Player.Jump.performed -= OnJump;
            input.InputActions.Player.Attack.performed -= OnAttack;
            input.InputActions.Player.Attack.canceled -= OnAttack;
            input.InputActions.Player.Spin.performed -= OnSpin;
            input.InputActions.Player.Spin.canceled -= OnSpin;
        }

        private void OnSpin(InputAction.CallbackContext ctx)
        {
            Animator.SetTrigger(ctx.performed ? SpinHash : StopSpinHash);
        }

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            Animator.SetTrigger(ctx.performed ? AttackHash : StopAttackHash);
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            Animator.SetTrigger(ctx.performed ? MoveHash : StopMoveHash);
        }

        private void OnRun(InputAction.CallbackContext ctx)
        {
            Animator.SetTrigger(ctx.performed ? RunHash : StopRunHash);
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            Animator.SetTrigger(JumpHash);
        }
    }
}