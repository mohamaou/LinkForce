using System;
using System.Collections;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Players;
using UnityEditor;
using UnityEngine;

namespace Zombies
{
    public enum PlayerTeam
    {
        Player1, Player2
    }

    public enum ZombieType
    {
        NormalZombie, FastZombie, ToughZombie, ShieldZombie, HelmetZombie, BarrelZombie, ZombieBoss1, ZombieBoss2, ZombieBoss3, ZombieBoss4
    }
    public class Zombie : MonoBehaviour
    {
        [Header("General")] 
        [SerializeField] private ZombieType zombieType;
        [SerializeField] private new ZombieAnimationManager animation;
        [SerializeField] private new Collider collider;
        [SerializeField] private Renderer[] renderers;

        [Header("Feedbacks")] 
        [SerializeField] private MMFeedbacks getHitFeedbacks;
        [SerializeField] private MMFeedbacks deathFeedbacks;
        
        [Header("Fight")]
        [SerializeField] private int health = 100;
        [SerializeField] private int damage = 10;
        
        [Header("Movement")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float movementSpeed;
        
        private PlayerTeam _targetPlayer;
        private Wall _targetWall;
        private bool _attacking, _stop;


        private void Start()
        {
            SetZombie();
        }


        public void SetRenders()
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        #region Public Events
        protected void SetZombie()
        {
            animation.SetAnimationsEvents(Attack);
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
        }
        public void SetTarget(PlayerTeam targetPlayer, bool natural)
        {
            tag = targetPlayer == PlayerTeam.Player1 ? "Player 1" : "Player 2";
            _targetPlayer = targetPlayer;
           // _targetWall = PlayersManager.Instance.GetTargetWall(targetPlayer);
            if (natural) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = targetPlayer == PlayerTeam.Player1 ? Color.red : Color.blue;
            }
        }
        protected void UpdateSpeed(float newSpeed)
        {
            movementSpeed = newSpeed;
        }
        protected PlayerTeam GetTargetedPlayer() => _targetPlayer;
        #endregion
       

        #region Public Variables
        public ZombieType GetZombieType() => zombieType;
        protected ZombieAnimationManager GetAnimation() => animation;
        #endregion


        #region Movement

        private void FixedUpdate()
        {
            var dir = (_targetWall.GetTargetPoint(transform.position) - transform.position).normalized;
            dir.y = 0;
          
            transform.rotation = Quaternion.LookRotation(dir);
        }
        protected IEnumerator StopForAMoment(float time)
        {
            _stop = true;
            yield return new WaitForSeconds(time);
            _stop = false;
        }


        #endregion
        
        

        #region Taking Damage

        public virtual void TakeDamage(int damageTaking)
        {
            health -= damageTaking;
            getHitFeedbacks?.PlayFeedbacks();
            if (health <= 0) KillMe();
        }

        protected virtual void KillMe()
        {
            if (!enabled) return;
            enabled = false;
            collider.enabled = false;
            gameObject.layer = 0;
            animation.Death();
            rb.isKinematic = true;
            deathFeedbacks?.PlayFeedbacks();
            ZombiesSpawner.Instance.ZombieDies(this);
            Invoke(nameof(HideBody),2f);
        }
        private void HideBody()
        {
            rb.isKinematic = false;
            Destroy(gameObject,2f);
        }

        #endregion
        
        #region Attacking
        private void OnTriggerEnter(Collider other)
        {
            if(!other.CompareTag("Player"))return;
            _attacking = true;
            animation.Attack(_attacking);
        }
        private void OnTriggerExit(Collider other)
        {
            if(!other.CompareTag("Player"))return;
            _attacking = false;
            animation.Attack(_attacking);
        }
        private void Attack()
        {
            //_targetWall.TakeDamage(damage);
        }
        #endregion
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(Zombie))]
    public class YourClassNameEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Zombie script = (Zombie)target;
            if (GUILayout.Button("Set Renders"))
            {
                script.SetRenders();
                EditorUtility.SetDirty(script);
            }
        }
    }
#endif
}
