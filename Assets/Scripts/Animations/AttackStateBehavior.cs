using Characters;
using UnityEngine;

namespace Animations
{
    public class AttackStateBehavior : StateMachineBehaviour
    {
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponent<CharacterHandler>();
            if (mover != null) mover.isAttack = true;
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponent<CharacterHandler>();
            if (mover != null) mover.isAttack = false;
        }
    }
}
