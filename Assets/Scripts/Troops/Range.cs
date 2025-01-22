using UnityEngine;

namespace Troops
{
    public class Range : Troop
    {
        [Header("Range")] 
        [SerializeField] private Arrow bullet;
        [SerializeField] private Transform shootPoint;

        
        public override void Attack(Troop target)
        {
            base.Attack(target);
            if (bullet != null)
            {
                var b = Instantiate(bullet); 
                b.transform.position = shootPoint.position; 
                b.transform.rotation = bullet.transform.rotation; 
               /// b.SetBullet(GetDamage(), target.transform.position + Vector3.up, GetTroopTeam());
            }
            else
            {
               // target.TakeDamage(GetDamage());
            }
        }
    }
}
