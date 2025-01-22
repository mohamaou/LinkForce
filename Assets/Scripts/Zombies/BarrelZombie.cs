using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Zombies
{
    public class BarrelZombie : Zombie
    {
        [Header("Barrel")] 
        [SerializeField] private ZombieType zombieToSpawn;
        [SerializeField] private GameObject[] heldZombies;
        [SerializeField] private GameObject[] barrelParts;
        [SerializeField] private Transform barrel;
        private bool _death;

        private void Start()
        {
            SetZombie();
            StartCoroutine(RotateBarrel());
        }

        private IEnumerator RotateBarrel()
        {
            while (true)
            {
                barrel.Rotate(0, -100*Time.deltaTime, 0);
                yield return null;
            }
        }
        
        
        private void OnTriggerEnter(Collider other)
        {
            if(!other.CompareTag("Player"))return;
            KillMe();
        }
        protected override void KillMe()
        {
            if (_death) return;
            _death = true;
            StopAllCoroutines();
            for (int i = 0; i < barrelParts.Length; i++)
            {
                var barrelPart = barrelParts[i];
                barrelPart.transform.SetParent(null);
                barrelPart.SetActive(true);
                barrelPart.transform.DOScale(Vector3.zero, 0.2f).SetDelay(1)
                    .OnComplete(() => Destroy(barrelPart));
            }
            for (int i = 0; i < heldZombies.Length; i++)
            {
                var spawnPos = new Vector3(barrel.position.x, transform.position.y, barrel.position.z);
                ZombiesSpawner.Instance.SpawnZombie(GetTargetedPlayer(),zombieToSpawn,spawnPos, true);
            }
            barrel.gameObject.SetActive(false);
            base.KillMe();
        }
    }
}
