using Players;
using UnityEngine;

namespace Animations
{
    public class JumpStateBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator,
            AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponentInParent<IMoveState>();
            if (mover != null) mover.IsJumping = true;
        }

        public override void OnStateExit(Animator animator,
            AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponentInParent<IMoveState>();
            if (mover != null) mover.IsJumping = false;
        }
    }
}