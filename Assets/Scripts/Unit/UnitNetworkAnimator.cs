using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unit
{
    public class UnitNetworkAnimator : NetworkAnimator
    {
        public static readonly int MoveHash = Animator.StringToHash("Move");
        public static readonly int RunHash = Animator.StringToHash("Run");
        public static readonly int JumpHash = Animator.StringToHash("Jump");
        public static readonly int SpinHash = Animator.StringToHash("Spin");
        public static readonly int AttackHash = Animator.StringToHash("Attack");
        public static readonly int HitHash = Animator.StringToHash("Hit");
        public static readonly int DeathHash = Animator.StringToHash("Death");

        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }

        internal void SetBool(int id, bool value)
        {
            Animator.SetBool(id, value);
        }

        internal void OnSpin(InputAction.CallbackContext ctx)
        {
            Animator.SetBool(SpinHash, ctx.performed);
        }

        internal void OnAttack(InputAction.CallbackContext ctx)
        {
            Animator.SetTrigger(AttackHash);
        }

        internal void OnMove(InputAction.CallbackContext ctx)
        {
            Animator.SetBool(MoveHash, ctx.performed);
        }

        internal void OnRun(InputAction.CallbackContext ctx)
        {
            Animator.SetBool(RunHash, ctx.performed);
        }

        internal void OnJump(InputAction.CallbackContext ctx)
        {
            Animator.SetTrigger(JumpHash);
        }

        internal void OnHit()
        {
            Animator.SetTrigger(HitHash);
        }

        internal void OnDeath()
        {
            Animator.SetTrigger(DeathHash);
        }

        internal void OnRebind()
        {
            Animator.Rebind();
        }
    }
}