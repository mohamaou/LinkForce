using UnityEngine;

namespace Troops
{
    public class Bomber : Troop
    {
        [SerializeField] private Bomb bomb;


        public override void Attack(Troop target)
        {
            base.Attack(target);
            var b = Instantiate(bomb);
            b.transform.position = bomb.transform.position;
            b.transform.rotation = bomb.transform.rotation;
            b.transform.localScale = Vector3.one;
           // b.SetBullet(GetDamage(),target.transform.position,GetTroopTeam());
        }
        
    }
}
