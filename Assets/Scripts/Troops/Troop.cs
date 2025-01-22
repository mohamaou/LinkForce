using System;
using AI;
using Core;
using MoreMountains.Feedbacks;
using Players;
using UnityEditor;
using UnityEngine;

namespace Troops
{
    public enum TroopType
    {
        Melee,
        Range,
        Bomber,
        Barracks,
        GoldMine,
        LegendaryBarracks,
        Prison,
        Rider,
        Trebuchet
    }

    public enum TroopState
    {
        Idle, Attacking, Moving, GoToEnemy, Death
    }

    public class Troop : TroopParent
    {
        [Header("General")] 
        [SerializeField] private TroopType troopType;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private new AnimationManager animation;
        [SerializeField] private Movement movement;
        [SerializeField] private LayerMask enemiesLayer;
        
        [SerializeField] private MMFeedbacks deathFeedbacks;
        [SerializeField] private MMFeedbacks attackFeedbacks;
        
        
        
        private TroopState _troopState = TroopState.Idle;
        private Action _onTroopSet, _onDeath;
        private bool _available;
        private Vector3 _healthOffset;
        

        #region Editor Code
        public void AssignComponents()
        {
            animation = GetComponentInChildren<AnimationManager>();
            healthBar = GetComponentInChildren<HealthBar>();
            gameObject.layer = 6;
            enemiesLayer = (1 << 6) | (1 << 7);
        }
        
        

        public void SetMovement()
        {
            movement = GetComponent<Movement>();
        }
        #endregion

        #region Public Variables
        
        
        public TroopType GetTroopType() => troopType;
        public TroopState GetTroopState() => _troopState;
        protected void SetTroopState(TroopState troopState)
        {
            if (_troopState == TroopState.Death) return;
            _troopState = troopState;
        }
        public AnimationManager GetAnimation() => animation;
        public Collider GetCollider() => GetComponent<Collider>();
        public void SetEvent(Action onTroopSet)=> _onTroopSet += onTroopSet;
        public void SetDeathEvent(Action onDeath)=> _onDeath += onDeath;
        public Movement GetMovement() => movement;
        #endregion

        #region Troop State
        protected bool CanMove()
        {
            if (_troopState == TroopState.Death) return false;
            if(_troopState == TroopState.Attacking) return false;
            return true;
        }
        protected bool IsEnemyInRange()
        {
            if(_troopState == TroopState.Death) return false;
            if (_troopState == TroopState.Attacking) return true;
            return false;
        }
        protected bool CanAttack()
        {
            if(_troopState == TroopState.Death) return false;
            if(_troopState == TroopState.GoToEnemy) return true;
            if (_troopState == TroopState.Moving) return true;
            return false;
        }
        public void SetAvailable(bool available) => _available = available;
        public bool IsAvailable() => _available;
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
            if(healthBar != null) healthBar.transform.position = transform.position + _healthOffset;
        }
        #endregion

        #region Death
        private void Death()
        {
            if (_troopState == TroopState.Death) return;
            animation?.Death();
            _onDeath?.Invoke();
            deathFeedbacks?.PlayFeedbacks();
            // if(_team == PlayerTeam.Player1) Bot.Instance.RemoveEnemyTroop(this);
            Destroy(healthBar.gameObject);
            GetComponent<Collider>().enabled = false;
            //Board.Instance.GetTile(transform.position).RemoveTroop();
            if(GetTroopTeam() == PlayerTeam.Player2) Bot.Instance.RemoveTroop(this);
            _troopState = TroopState.Death;
            GetComponent<Rigidbody>().isKinematic = true;
            GameManager.Instance.CheckIfGameEnds(GetTroopTeam());
            Invoke(nameof(HideBody),2f);
        }
        private void HideBody()
        {
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            Destroy(gameObject,1);
            //   Arena.Instance.BuildNavMeshDelay();
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

        Troop tower = (Troop)target;

        if (GUILayout.Button("Assign Components"))
        {
            foreach (var t in targets)
            {
                Troop eachTower = (Troop)t;
                eachTower.AssignComponents();
                EditorUtility.SetDirty(eachTower);
            }
        }

        if (GUILayout.Button("Setup Rigidbody & Collider"))
        {
            foreach (var obj in targets)
            {
                Troop eachTower = (Troop)obj;
                SetupRigidbodyAndCollider(eachTower.gameObject);
                EditorUtility.SetDirty(eachTower);
            }
        }

        if (GUILayout.Button("Setup Outline"))
        {
            foreach (var obj in targets)
            {
                Troop eachTower = (Troop)obj;
                EditorUtility.SetDirty(eachTower);
            }
        }
        
        if (GUILayout.Button("Set Movement"))
        {
            foreach (var obj in targets)
            {
                Troop eachTower = (Troop)obj;
                eachTower.SetMovement();
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
            col.height = 2f;
            col.radius = 0.3f;
            var center = col.center;
            center.y = 1f;
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
