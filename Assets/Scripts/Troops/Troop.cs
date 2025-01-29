using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using Cards;
using Core;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Players;
using UnityEditor;
using UnityEngine;

namespace Troops
{
    public enum DamageType
    {
        Physical, Poison, Ice, Lightning
    }
    [Serializable]
    public struct Damage
    {
        [SerializeField] private float damage;
        [SerializeField] private DamageType damageType;

        public Damage(float damage, DamageType damageType)
        {
            this.damage = damage;
            this.damageType = damageType;
        }

        public float GetDamageAmount() => damage;
        public DamageType GetDamageType() => damageType;
    }
    
    public enum TroopState
    {
        Idle,
        Attacking,
        Moving,
        Death
    }

    public enum TroopType
    {
        Assassin,Berserker,Ghost,Rocket,Armor,Shield,Axe,Bow,Electric 
        ,Ice,Poison,Sword,Zombie,Human,Skeleton,Goblin
    }
    
    [Serializable]
    public class Equipment
    {
        [SerializeField] private TroopType id;
        [SerializeField] private bool hideOriginalBody, changeAnimator;
        [SerializeField] private GameObject[] equipments;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private RuntimeAnimatorController  controller;
        [SerializeField] private Material player1Material, player2Material;
        [SerializeField] private BuildingCard buildingCard;
        [SerializeField] private Projectile projectile;
        [SerializeField] private Transform shootPoint;
        
        private float _range;
        private Damage _damage;
        private BuildingType _buildingType;

        //Visual
        public void SetVisualColors(PlayerTeam team)
        {
            foreach (var r in renderers)
            {
                r.material = team == PlayerTeam.Player1 ? player1Material : player2Material;
            }
        }
        public void ShowVisual(bool show)
        {
            foreach (var equipment in equipments)
            {
                equipment.SetActive(show);
            }
        }
        public bool ChangeAnimation() => changeAnimator;
        public bool HideOriginalBody() => hideOriginalBody;
        public RuntimeAnimatorController GetController() => controller;

        public void SetBodyColor(Color color)
        {
            foreach (var renderer in renderers)
            {
                renderer.material.SetColor("_BaseColor", color);
            }
        }
        
        
        //Stats
        public void SetData(int level)
        {
            _range = buildingCard.GetRange();
            _buildingType = buildingCard.GetBuildingType();
            _damage = buildingCard.GetDamage(level);
        }
        public float GetRange() => _range;
        public BuildingType GetBuildingType() => _buildingType;
        public TroopType GetEquipmentType() => id;
        public Damage GetDamage() => _damage;
        
        //Weapons
        public Projectile GetProjectile() => projectile;
        public Transform GetShootPoint() => shootPoint;

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
        
        [Header("Equipment")]
        [SerializeField] private Equipment originalEquipment;
        [SerializeField] private Equipment[] equipments;
        
        [Header("Damage Effects")]
        [SerializeField] private GameObject electrocutedEffect;
        [SerializeField] private GameObject poisonedEffect, freezeEffect;
     
        private TroopState _troopState = TroopState.Idle;
        private Action _onTroopSet, _onDeath;
        private bool _available;
        private Vector3 _healthOffset;
        private PlayerTeam _team;
        private readonly List<Equipment> _equipmentsWeHave = new();

        private bool _ice, _lighting, _poisoned;
        
        [Header("For Testing")]
        public PlayerTeam test;
        public TroopType[] equipmentsTest;
        

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

        #region Start
        private void Start()
        {
            poisonedEffect.SetActive(false);
            freezeEffect.SetActive(false);
            electrocutedEffect.SetActive(false);
            StartCoroutine(HealthBar());
            StartCoroutine(Fight());
            originalEquipment.SetData(0);
            foreach (var equipment in equipments)
            {
                equipment.SetData(0);
            }

            //This is Only For Testing
            if (test == PlayerTeam.Player1) Player.Instance.AddTroop(this);
            else Bot.Instance.AddTroop(this);
            SetTroop(test);
            foreach (var e in equipmentsTest)
            {
                SetEquipment(e);
            }
        }
        private IEnumerator HealthBar()
        {
            if(healthBar == null) yield break;
            while (_troopState != TroopState.Death)
            {
                healthBar.transform.position = transform.position + _healthOffset;
                yield return null;
            }
        }
        public void SetDeathEvent(Action onDeath) => _onDeath += onDeath;

        public void SetTroop(PlayerTeam team)
        {
            _team = team;
            transform.tag = team == PlayerTeam.Player1 ? "Player 1" : "Player 2";
            originalEquipment.SetVisualColors(team);
            animation.SetController(originalEquipment.GetController());
        }
        #endregion

        #region Fight
        public void SetEquipment(TroopType equipment)
        {
            foreach (var e in equipments)
            {
                if (equipment == e.GetEquipmentType())
                {
                    _equipmentsWeHave.Add(e);
                    e.SetVisualColors(team:_team);
                    if(e.ChangeAnimation()) animation.SetController(e.GetController());
                    e.ShowVisual(true);
                    if (e.HideOriginalBody()) originalEquipment.ShowVisual(false);
                }
            }
        }

        private IEnumerator Fight()
        {
            if (test != PlayerTeam.Player1) yield break;
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Battle);
            while (TurnsManager.PlayState == PlayState.Battle && _troopState != TroopState.Death)
            {
                var closestTroop = GetClosestEnemy();
                var dist = Vector3.Distance(transform.position, closestTroop.transform.position);
                if (dist < GetRange())
                {
                    yield return Attack();
                }
                yield return null;
            }
        }
        private IEnumerator Attack()
        {
            yield return RotateTowards();
            
            var attack = false;
            if (GetWeapon().GetEquipmentType() == TroopType.Axe)
            {
                transform.GetChild(0).DOKill();
                transform.GetChild(0).DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear);
            }
            animation.AttackAnimation(() =>
            {
                attack = true;
            });
            
            yield return new WaitUntil(() => attack);
       
            var weapon = GetWeapon();
            switch (weapon.GetEquipmentType())
            {
                case TroopType.Human:
                case TroopType.Skeleton:
                case TroopType.Zombie:
                case TroopType.Goblin:
                case TroopType.Sword:
                    MeleeAttack(weapon);
                    break;
                case TroopType.Poison:
                case TroopType.Electric:
                case TroopType.Bow:
                case TroopType.Ice:
                    RangeAttack(weapon);
                    break;
                case TroopType.Axe:
                    AOEAttack(weapon);
                    break;
            }

            var waiteTime = weapon.GetDamage().GetDamageType() switch
            {
                DamageType.Lightning=> 3,
                _=>1
            };
            yield return new WaitForSeconds(waiteTime);
        }
        private IEnumerator RotateTowards()
        {
            Vector3 targetPosition = GetClosestEnemy().transform.position;
            targetPosition.y = transform.position.y;
            float rotationSpeed = 180f;
            Vector3 directionToEnemy = (targetPosition - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

            while (angleDifference > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
                yield return null;
            }
        }
        
        private void MeleeAttack(Equipment weapon)
        {
            var enemy = GetClosestEnemy();
            if (Vector3.Distance(transform.position, enemy.transform.position) > weapon.GetRange()) return;
            enemy.TakeDamage(weapon.GetDamage());
        }
        private void RangeAttack(Equipment weapon)
        {
            var shootPoint = weapon.GetShootPoint();
            var projectile = Instantiate(weapon.GetProjectile(),shootPoint.position, shootPoint.rotation);
            projectile.SetProjectile(GetClosestEnemy().transform.position+Vector3.up, weapon.GetDamage(),_team);
        }
        public void AOEAttack(Equipment equipment)
        {
            foreach (var enemy in _team == PlayerTeam.Player1? Bot.Instance.GetTroops():Player.Instance.GetTroops() )
            {
                if (Vector3.Distance(enemy.transform.position, transform.position) <= equipment.GetRange())
                {
                    enemy.TakeDamage(equipment.GetDamage());
                }
            }
        }
        
        
        private Troop GetClosestEnemy()
        {
            Troop closestTroop = null;
            var closestDistance = float.MaxValue;
            foreach (var enemyTroop in _team == PlayerTeam.Player1?Bot.Instance.GetTroops() : Player.Instance.GetTroops())
            {
                var dist = Vector3.Distance(transform.position, enemyTroop.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestTroop = enemyTroop;
                }
            }
            return closestTroop;
        }
        private float GetRange()
        {
            foreach (var equipment in _equipmentsWeHave)
            {
                if (equipment.GetBuildingType() == BuildingType.Weapon)
                {
                    
                    return equipment.GetRange();
                }
            }
            return originalEquipment.GetRange();
        }
        private Equipment GetWeapon()
        {
            foreach (var equipment in _equipmentsWeHave)
            {
                if (equipment.GetBuildingType() == BuildingType.Weapon) return equipment;
            }
            return originalEquipment;
        }
        #endregion

        #region Take Damage
        public void TakeDamage(Damage damage)
        {
            print("Take Damage");
            switch (damage.GetDamageType())
            {
                case DamageType.Ice: 
                    if(!_ice)StartCoroutine(IceDamage());
                    break;
                case DamageType.Lightning: 
                    if(!_lighting)StartCoroutine(LightingDamage());
                    break;
                case DamageType.Poison: 
                    if(!_poisoned)StartCoroutine(PoisonDamage());
                    break;
            }
        }

        private IEnumerator LightingDamage()
        {
            _lighting = true;
            electrocutedEffect.SetActive(true);
            animation.Electrocuted(true);
            yield return new WaitForSeconds(1.5f);
            _lighting = false;
            animation.Electrocuted(false);
            electrocutedEffect.SetActive(false);
        }
        private IEnumerator PoisonDamage()
        {
            _poisoned = true;
            poisonedEffect.SetActive(true);
            SetBodyColor(Color.green);
            yield return new WaitForSeconds(5f);
            _poisoned = false;
            SetBodyColor(Color.white);
            poisonedEffect.SetActive(false);
        }
        private IEnumerator IceDamage()
        {
            _ice = true;
            freezeEffect.SetActive(true);
            SetBodyColor(Color.cyan);
            animation.SetAnimationSpeed(0.5f);
            yield return new WaitForSeconds(5f);
            animation.SetAnimationSpeed(1);
            _ice = false;
            SetBodyColor(Color.white);
            freezeEffect.SetActive(false);
        }

        private void SetBodyColor(Color color)
        {
            foreach (var equipment in _equipmentsWeHave)
            {
                equipment.SetBodyColor(color);
            }
        }
        
        private void Death()
        {
            if (_troopState == TroopState.Death) return;
            _onDeath?.Invoke();
            deathFeedbacks?.PlayFeedbacks();
            // if(_team == PlayerTeam.Player1) Bot.Instance.RemoveEnemyTroop(this);
            Destroy(healthBar.gameObject);
            GetComponent<Collider>().enabled = false;
           // if (_team == PlayerTeam.Player2) Bot.Instance.RemoveTroop(this);
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
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Assign Components"))
            {
                foreach (var t in targets)
                {
                    var eachTower = (Troop)t;
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