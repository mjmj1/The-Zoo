using Characters;
using UnityEngine;

namespace Animations
{
    public class JumpStateBehavior : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            var mover = animator.GetComponent<CharacterHandler>();
            if (mover != null) mover.isGround = false;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponent<CharacterHandler>();
            if (mover != null) mover.isGround = true;
        }
    }
}
