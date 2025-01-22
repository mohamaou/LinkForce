using Players;
using UnityEngine;


namespace Troops
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private new Collider collider;
        [SerializeField] private TrailRenderer trail;
        private float _damage;
        private PlayerTeam _team;

        protected float GetDamage() => _damage;
        
        
        public virtual void SetBullet(float damage, Vector3 target, PlayerTeam team)
        {
            _team = team;
            enabled = true;
            _damage = damage;
            collider.enabled = true;
            if (trail != null) trail.enabled = true;
        }
        public virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != 6 && other.gameObject.layer != 7) return;
            if (!enabled) return;
            if (other.CompareTag(_team == PlayerTeam.Player1 ? "Player 1" : "Player 2")) return; 
            Destroy(gameObject);
            if(other.gameObject.layer == 6) other.gameObject.GetComponent<Troop>().TakeDamage(_damage);
            else if(other.gameObject.layer == 7) other.gameObject.GetComponentInParent<Base>().TakeDamage(_damage);
        }
        
        protected string GetTargetTag()=> _team== PlayerTeam.Player1 ? "Player 2" : "Player 1";
    }
}
