using Players;
using UnityEngine;

namespace Animations
{
    public class SpinStateBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator,
            AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponentInParent<IMoveState>();
            if (mover != null) mover.IsSpinning = true;
        }

        public override void OnStateExit(Animator animator,
            AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponentInParent<IMoveState>();
            if (mover != null) mover.IsSpinning = false;
        }
    }
}