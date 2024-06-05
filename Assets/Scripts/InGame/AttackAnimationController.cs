using UnityEngine;

namespace InGame
{
    public class AttackAnimationController : MonoBehaviour
    {
        public Animator animator; // Reference to the Animator component
        public AnimatorOverrideController overrideController; // Reference to the Animator Override Controller
        public AnimationClip[] attackClips; // Array of attack animation clips

        private int currentAttackClipIndex = 0;

        void Awake()
        {
            // Initialize the Animator and assign the override controller
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = overrideController;
        
            ChangeAttackAnimation(currentAttackClipIndex);
        
        }

        void ChangeAttackAnimation(int index)
        {
            if (index >= 0 && index < attackClips.Length)
            {
                // Override the default attack animation with the selected clip
                currentAttackClipIndex = index;
                overrideController["Attack"] = attackClips[index];
            }
            else
            {
                Debug.LogWarning("Invalid attack clip index");
            }
        }
    }
}
