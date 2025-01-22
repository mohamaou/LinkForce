using UnityEngine;

namespace Zombies
{
    public class ZombieAnimationManager : MonoBehaviour
    {
        [SerializeField] private new Animator animation;
        private System.Action _attack;

        public void SetAnimationsEvents(System.Action attackEvent)
        {
            _attack = attackEvent;
        }

        public void Attack(bool attack)
        {
            if (!enabled) return;
            animation.SetBool("Attack", attack);
        }
        public void Death()
        {
            enabled = false;
            animation.SetTrigger("Death");
        }

        public void ShieldDropped()
        {
            animation.SetTrigger("Shield Dropped");
        }
        
        
        public void AttackEvent()=> _attack?.Invoke();
    }
}
