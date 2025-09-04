using Players;
using Unit;
using UnityEngine;

namespace Animations
{
    public class MoveStateBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponentInParent<IActionState>();
            if (mover != null) mover.CanMove = false;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponentInParent<IActionState>();
            if (mover != null) mover.CanMove = true;
        }
    }
}