using MoreMountains.Feedbacks;
using Players;
using UnityEngine;

namespace Troops
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TrailRenderer))]
    public class Bomb : Weapon
    {
        [SerializeField] private LayerMask zombiesLayer;
       
        [SerializeField] private Rigidbody rb;
        [SerializeField] private MMFeedbacks explosionFeedbacks;
        [SerializeField] private float explosionRadius = 2f;

        private void Start()
        {
            
        }

        public override void SetBullet(float damage, Vector3 target, PlayerTeam team)
        {
            base.SetBullet(damage, target, team);
            rb.isKinematic = false;
            var direction = target - transform.position;
            var height = direction.y;
            direction.y = 0;
            var distance = direction.magnitude;
            direction.y = distance;
            distance += height;
            var velocity = Mathf.Sqrt(distance * Physics.gravity.magnitude);
            rb.linearVelocity = velocity * direction.normalized;
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (!enabled) return;
            if (other.gameObject.layer == 3)
            {
                Explode();
                explosionFeedbacks?.transform.SetParent(null);
                Destroy(explosionFeedbacks,5f);
            }
        }
        
        private void Explode() 
        {
            explosionFeedbacks?.PlayFeedbacks();
            var hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var hitCollider in hitColliders)
            {
                var zombie = hitCollider.GetComponent<Troop>();
                if (zombie != null)
                {
                    if(zombie.CompareTag(GetTargetTag()))zombie.TakeDamage(GetDamage());
                }
                var b = hitCollider.GetComponentInParent<Base>();
                if(b!= null && b.CompareTag(GetTargetTag()))b.TakeDamage(GetDamage());
            }
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position,explosionRadius);
        }
    }
}
