using UnityEngine;
using Random = UnityEngine.Random;

namespace Troops
{
    public class AnimationManager : MonoBehaviour
    {
       [SerializeField] private Animator animator, horseAnimation;


       private void Start()
       {
           animator.applyRootMotion = false;
       }

       public void Placed()
       {
           animator.SetTrigger("Place");
           if(horseAnimation != null) horseAnimation.SetTrigger("Place");
       }

       public void Move(bool move)
       {
           animator.SetBool("Idle", !move);
           if(horseAnimation != null) horseAnimation.SetBool("Idle", !move);
       }

       public void Attack()
       {
           animator.SetTrigger("Attack");
           float[] possibleValues = {0f, 0.5f, 1f};
           var chosenValue = possibleValues[Random.Range(0, possibleValues.Length)];
           animator.SetFloat("Random", chosenValue);

       }

       public void Death()
       {
           animator.SetTrigger("Death");
           if(horseAnimation != null) horseAnimation.SetTrigger("Death");
       }
    }
}
