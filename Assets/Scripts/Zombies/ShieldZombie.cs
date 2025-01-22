using System;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Zombies
{
    public class ShieldZombie : Zombie
    {
        [Serializable]
        public struct Shield
        {
            public Rigidbody rb;
            public Collider collider;
        }
        
        [Header("Shield")] 
        [SerializeField] private float unshieldedSpeed;
        [SerializeField] private int shieldedHealth = 100;
        [SerializeField] private Shield shield;
        [SerializeField] private MMFeedbacks shieldGetHitFeedbacks;
        private bool _hasShield;
        
        
        private void Start()
        {
            SetZombie();
        }


        public override void TakeDamage(int damageTaking)
        {
            shieldedHealth -= damageTaking;
            if (shieldedHealth <= 0)
            {
                if (!_hasShield)
                {
                    DropShield();
                    _hasShield = true;
                }
                base.TakeDamage(damageTaking);
            }
            else
            {
                shieldGetHitFeedbacks.PlayFeedbacks();
            }
        }
        private void DropShield()
        {
            GetAnimation().ShieldDropped();
            UpdateSpeed(unshieldedSpeed);
            StartCoroutine(StopForAMoment(1));
            shield.collider.transform.SetParent(null);
            shield.rb.isKinematic = false; 
            Destroy(shield.collider,5f);
            shield.rb.AddForce(transform.up * 5f - transform.forward * 2f, ForceMode.Impulse); 
            shield.collider.enabled = true;
        }
    }
}
