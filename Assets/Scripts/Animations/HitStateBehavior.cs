using Characters;
using Characters.Roles;
using UnityEngine;

namespace Animations
{
    public class HitStateBehavior : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            var mover = animator.GetComponent<CharacterHandler>();
            if (mover != null) mover.isHit = true;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mover = animator.GetComponent<CharacterHandler>();
            if (mover != null) mover.isHit = false;
        }
    }
}
