using UnityEngine;

namespace Troops
{
    public class AnimationManager : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private System.Action _attack;


        public void AttackAnimation(System.Action attack)
        {
            animator.SetTrigger("Attack");
            _attack = attack;
        }

        public void SetAnimationSpeed(float speed)
        {
            animator.speed = speed;
        }
        public void Electrocuted(bool active)
        {
            animator.SetBool("Electrocuted", active);
        }
        
        public void SetController(RuntimeAnimatorController controller)
        {
            animator.runtimeAnimatorController = controller;
        }

        public void Attack()
        {
            _attack?.Invoke();
        }
    }
}
