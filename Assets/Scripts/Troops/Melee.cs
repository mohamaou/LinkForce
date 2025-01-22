

namespace Troops
{
    public class Melee : Troop
    {
        public override void Attack(Troop target)
        {
            base.Attack(target); 
           // target.TakeDamage(GetDamage());
        }
    }
}
