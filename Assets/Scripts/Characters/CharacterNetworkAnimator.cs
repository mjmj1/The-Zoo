using Unity.Netcode.Components;

namespace Characters
{
    public class CharacterNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }

        public void SetTrigger(int id)
        {
            Animator.SetTrigger(id);
        }
        
        public void SetBool(int id, bool value)
        {
            Animator.SetBool(id, value);
        }

        public void SetFloat(int id, float value)
        {
            Animator.SetFloat(id, value);
        }
    }
}