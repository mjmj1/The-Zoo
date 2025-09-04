using Players;
using Unit;
using UnityEngine;

namespace Animations
{
    public class JumpStateBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator,
            AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponentInParent<IActionState>();
            if (mover != null) mover.IsJumping = true;
        }

        public override void OnStateExit(Animator animator,
            AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponentInParent<IActionState>();
            if (mover != null) mover.IsJumping = false;
        }
    }
}