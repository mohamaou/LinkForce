using DG.Tweening;
using Players;
using UnityEngine;
namespace Troops
{
    public class Arrow : Weapon
    {
        [SerializeField] private float speed = 24f;
        [SerializeField] private new DOTweenAnimation animation;



        public override void SetBullet(float damage, Vector3 target, PlayerTeam team)
        {
            base.SetBullet(damage, target, team);
            transform.LookAt(target);
            if (animation != null) animation.DOPlay();
        }
        
        private void Update()
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
        }
    }
}
