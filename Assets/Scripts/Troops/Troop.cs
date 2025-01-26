using System;
using AI;
using Core;
using MoreMountains.Feedbacks;
using Players;
using UnityEditor;
using UnityEngine;

namespace Troops
{
    public enum TroopState
    {
        Idle,
        Attacking,
        Moving,
        Death
    }

    [Serializable]
    public class Equipment
    {
        [SerializeField] private string id;
<<<<<<< HEAD
        [SerializeField] private bool hideOriginalBody, changeAnimator;
        [SerializeField] private GameObject[] equipments;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private RuntimeAnimatorController  controller;
=======
        [SerializeField] private bool hideOriginalBody;
        [SerializeField] private GameObject equipment;
        [SerializeField] private Animator animator;
        [SerializeField] private Renderer[] renderers;
>>>>>>> ab2bb7a9350fb735e856642b24695d7e79bcf020
        [SerializeField] private Material player1Material, player2Material;
    }

    public class Troop : MonoBehaviour
    {
        [SerializeField] private new Collider collider;
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private new AnimationManager animation;
        [SerializeField] private Movement movement;
        [SerializeField] private LayerMask enemiesLayer;
        [SerializeField] private MMFeedbacks deathFeedbacks;
<<<<<<< HEAD
        
        [Header("Equipment")]
        [SerializeField] private Equipment originalEquipment;
=======

        [Header("Equipment")] [SerializeField] private Equipment originalEquipment;
>>>>>>> ab2bb7a9350fb735e856642b24695d7e79bcf020
        [SerializeField] private Equipment[] equipments;
     
        private TroopState _troopState = TroopState.Idle;
        private Action _onTroopSet, _onDeath;
        private bool _available;
        private Vector3 _healthOffset;
        private PlayerTeam _team;


        #region Editor Code

        public void AssignComponents()
        {
            animation = GetComponentInChildren<AnimationManager>();
            healthBar = GetComponentInChildren<HealthBar>();
            rigidbody = GetComponentInChildren<Rigidbody>();
            collider = GetComponentInChildren<Collider>();
            if (GetComponentInChildren<AnimationManager>() != null)
                animation = GetComponentInChildren<AnimationManager>();
            if (GetComponentInChildren<Movement>() != null)
                movement = GetComponentInChildren<Movement>();
            gameObject.layer = 6;
            enemiesLayer = (1 << 6);
        }

        #endregion

        #region Public Variables

        public TroopState GetTroopState() => _troopState;

        protected void SetTroopState(TroopState troopState)
        {
            if (_troopState == TroopState.Death) return;
            _troopState = troopState;
        }

        public AnimationManager GetAnimation() => animation;
        public Collider GetCollider() => GetComponent<Collider>();
        public void SetEvent(Action onTroopSet) => _onTroopSet += onTroopSet;
        public void SetDeathEvent(Action onDeath) => _onDeath += onDeath;
        public Movement GetMovement() => movement;

        #endregion

        #region Fighting

        public void TakeDamage(float damage)
        {
            if (_troopState == TroopState.Death) return;

            healthBar.TakeDamage(damage);
        }

        public virtual void Attack(Troop target)
        {
        }

        private void Update()
        {
            if (healthBar != null) healthBar.transform.position = transform.position + _healthOffset;
        }

        #endregion

        #region Death

        private void Death()
        {
            if (_troopState == TroopState.Death) return;
            _onDeath?.Invoke();
            deathFeedbacks?.PlayFeedbacks();
            // if(_team == PlayerTeam.Player1) Bot.Instance.RemoveEnemyTroop(this);
            Destroy(healthBar.gameObject);
            GetComponent<Collider>().enabled = false;
            if (_team == PlayerTeam.Player2) Bot.Instance.RemoveTroop(this);
            _troopState = TroopState.Death;
            GetComponent<Rigidbody>().isKinematic = true;
            GameManager.Instance.CheckIfGameEnds(_team);
            Invoke(nameof(HideBody), 2f);
        }

        private void HideBody()
        {
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            Destroy(gameObject, 1);
        }

        #endregion
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Troop), true)]
    public class TowerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
<<<<<<< HEAD
            
            GUILayout.Space(20);
            
=======

            GUILayout.Space(20);

>>>>>>> ab2bb7a9350fb735e856642b24695d7e79bcf020
            if (GUILayout.Button("Assign Components"))
            {
                foreach (var t in targets)
                {
<<<<<<< HEAD
                    var eachTower = (Troop)t;
=======
                    var eachTower = (Troop) t;
>>>>>>> ab2bb7a9350fb735e856642b24695d7e79bcf020
                    eachTower.AssignComponents();
                    EditorUtility.SetDirty(eachTower);
                }
            }

            if (GUILayout.Button("Setup Rigidbody & Collider"))
            {
                foreach (var obj in targets)
                {
<<<<<<< HEAD
                    Troop eachTower = (Troop)obj;
=======
                    Troop eachTower = (Troop) obj;
>>>>>>> ab2bb7a9350fb735e856642b24695d7e79bcf020
                    SetupRigidbodyAndCollider(eachTower.gameObject);
                    EditorUtility.SetDirty(eachTower);
                }
            }
        }

        private void SetupRigidbodyAndCollider(GameObject go)
        {
            // Check for existing rigidbody
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = Undo.AddComponent<Rigidbody>(go);
                rb.mass = 80;
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }
            else
            {
                // If already exists, ensure the correct values are set
                rb.mass = 80;
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }

            // Check for existing capsule collider
            CapsuleCollider col = go.GetComponent<CapsuleCollider>();
            if (col == null)
            {
                col = Undo.AddComponent<CapsuleCollider>(go);
                col.isTrigger = true;
                col.height = 2.2f;
                col.radius = 0.35f;
                var center = col.center;
                center.y = 1.1f;
                col.center = center;
            }
            else
            {
                // If it already exists, just ensure values are correct
                col.height = 2f;
                col.radius = 0.3f;
                var center = col.center;
                center.y = 1f;
                col.center = center;
            }
        }
    }
#endif
}