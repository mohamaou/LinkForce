using System.Collections;
using Players;
using UnityEngine;

namespace Troops
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10;
        [SerializeField] private GameObject explosionEffect;
        private Vector3 _target;
        private Damage _damage;
        private PlayerTeam _team;

        public void SetProjectile(Vector3 enemy, Damage damage, PlayerTeam team)
        {
            _target = enemy;
            StartCoroutine(MoveProjectile());
            _damage = damage;
            _team = team;
        }
        private IEnumerator MoveProjectile()
        {
            var startPos = transform.position;
            var distance = Vector3.Distance(startPos, _target);
            var travelTime = distance / speed;
            var elapsedTime = 0f;

            var previousPos = transform.position; 

            while (elapsedTime < travelTime)
            {
                elapsedTime += Time.deltaTime;
                var progress = elapsedTime / travelTime;
                
                var newPos = Vector3.Lerp(startPos, _target, progress);
                newPos.y += Mathf.Sin(progress * Mathf.PI) * 2;
                
                transform.LookAt(newPos + (newPos - previousPos));

                previousPos = transform.position;
                transform.position = newPos;
                yield return null;
            }
            DestroyMe();
        }

        private void DestroyMe()
        {
            Destroy(gameObject);
            if (explosionEffect == null) return;
            var e =Instantiate(explosionEffect,transform.position,Quaternion.identity);
            Destroy(e,2f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 6)
            {
                if (other.CompareTag(_team == PlayerTeam.Player1 ? "Player 2" : "Player 1"))
                {
                    other.GetComponent<Troop>().TakeDamage(_damage);
                    DestroyMe();
                }
            }
        }
    }
}
