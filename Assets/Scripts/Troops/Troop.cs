using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Core;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Players;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Troops
{
    public enum DamageType
    {
        Physical,
        Poison,
        Ice,
        Lightning
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
        Building, Idle, Attacking, Moving, Death
    }
    public enum TroopType
    {
        Assassin, Berserker, Ghost, Rocket, Armor, Shield, Axe,
        Bow, Electric, Ice, Poison, Sword, Zombie, Human, Skeleton, Goblin
    }
    [Serializable]
    public class Equipment
    {
        [SerializeField] private TroopType id;
        [SerializeField] private bool hideOriginalBody, changeAnimator;
        [SerializeField] private GameObject[] equipments;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private RuntimeAnimatorController controller;
        [SerializeField] private Material player1Material, player2Material;
        [SerializeField] private BuildingCard buildingCard;
        [SerializeField] private Projectile projectile;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private Shield shield;
        [SerializeField] private Ghost ghost;
        [SerializeField] private RocketLauncher rocket;

        private float _range, _assassinHideTime, _armor, _health, _healthIncrease, _damageIncrease;
        private Damage _damage;
        private BuildingType _buildingType;

        #region Visuals
         public void SetVisualColors(PlayerTeam team)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if(renderers[i] != null) renderers[i].material = team == PlayerTeam.Player1 ? player1Material : player2Material;
            }
            if(id != TroopType.Ghost) SetBodyColor(Color.white);
        }
        public void ShowVisual(bool show)
        {
            foreach (var equipment in equipments)
            {
                equipment.SetActive(show);
            }
            if(shield != null) shield.gameObject.SetActive(show);
        }
        public bool ChangeAnimation() => changeAnimator;
        public bool HideOriginalBody() => hideOriginalBody;
        public RuntimeAnimatorController GetController() => controller;
        public void SetBodyColor(Color color)
        {
            color.a = .8f;
            foreach (var renderer in renderers)
            {
                if(renderer != null) renderer.material.SetColor("_BaseColor", color);
            }
        }
        public void Hide(bool hide)
        {
            foreach (var renderer in renderers)
            {
                SetMaterialTransparency(renderer.material, hide);
            }
        }
        private void SetMaterialTransparency(Material material, bool isTransparent)
        {
            if (isTransparent)
            {
                material.SetFloat("_RenderingMode", 1); // Transparent Mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0); // Disable ZWrite for proper transparency
                material.EnableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                material.SetFloat("_RenderingMode", 0); // Opaque Mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1); // Enable ZWrite for opaque rendering
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            }
        }

        #endregion

        #region Stats
        public void SetData(int level,PlayerTeam team,TroopType equipmentType)
        {
            if (equipmentType != id) return;
            _buildingType = buildingCard.GetBuildingType();
            switch (equipmentType)
            {
                case TroopType.Human:
                case TroopType.Zombie:
                case TroopType.Goblin:
                case TroopType.Skeleton:
                    _health = buildingCard.GetHealth(level);
                    _range = buildingCard.GetRange();
                    _damage = buildingCard.GetDamage(level);
                    break;
                case TroopType.Axe:
                case TroopType.Bow:
                case TroopType.Sword:
                case TroopType.Electric:
                case TroopType.Ice:
                case TroopType.Poison:
                    _range = buildingCard.GetRange();
                    _damage = buildingCard.GetDamage(level);
                    break;
                case TroopType.Shield:
                    shield.SetShields(buildingCard.GetShieldsCount(level));
                    break;
                case TroopType.Ghost:
                    ghost.SetGhost(team, buildingCard.GetGhostDamage(level));
                    break;
                case TroopType.Rocket:
                    rocket.SetupRocket(team,buildingCard.GetRocketDamage(level));
                    break;
                case TroopType.Assassin:
                    _assassinHideTime = buildingCard.GetAssassinHideTime(level);
                    break;
                case TroopType.Armor:
                    _armor = buildingCard.GetArmorDamageNegation(level);
                    break;
                case TroopType.Berserker:
                    _damageIncrease = buildingCard.GetBerserkerDamageIncrease(level);
                    _healthIncrease = buildingCard.GetBerserkerHealthIncrease(level);
                    break;
            }
        }

        public float GetHealth() => _health;
        public float GetRange() => _range;
        public BuildingType GetBuildingType() => _buildingType;
        public TroopType GetEquipmentType() => id;
        public Damage GetDamage() => _damage;
        public float GetAssassinHideTime() => _assassinHideTime;
        public float GetDamageIncrease() => _damageIncrease;
        public float GetHealthIncrease() => _healthIncrease;
        #endregion
        

        //Weapons
        public Projectile GetProjectile() => projectile;
        public Transform GetShootPoint() => shootPoint;

        //Buffs
        public Shield GetShield() => shield;
        public Ghost GetGhost() => ghost;
        public RocketLauncher GetRocketLauncher() => rocket;
        public float GetDamageNegation() => _armor;
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

        [Header("Equipment")] [SerializeField] private Equipment originalEquipment;
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
        private float _health, _damageIncrease, _maxHealth;

        
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
            StartCoroutine(Fight());
        }
        public void SetDeathEvent(Action onDeath) => _onDeath += onDeath;
        #endregion

        #region Bublic Varialbles
        public PlayerTeam GetTeam() => _team;
        public TroopState GetTroopStat()=> _troopState;
        public Movement GetMovement() => movement;
        public Troop GetClosestEnemy()
        {
            Troop closestTroop = null;
            var closestDistance = float.MaxValue;
            foreach (var enemyTroop in _team == PlayerTeam.Player1
                         ? Bot.Instance.GetTroops()
                         : Player.Instance.GetTroops())
            {
                var dist = Vector3.Distance(transform.position, enemyTroop.transform.position);
                if (dist < closestDistance && enemyTroop.gameObject.layer == 6)
                {
                    closestDistance = dist;
                    closestTroop = enemyTroop;
                }
            }

            return closestTroop;
        }
        public Rigidbody GetRigidbody() => rigidbody;
        public AnimationManager GetAnimator() => animation;
        public float GetHealth() => _health;
        #endregion
        
        #region Building
        public void SetTroop(PlayerTeam team, int level)
        {
            _team = team;
            var troopType =  originalEquipment.GetEquipmentType();
            originalEquipment.SetData(level-1, team, troopType);
            transform.tag = team == PlayerTeam.Player1 ? "Player 1" : "Player 2";
            originalEquipment.SetVisualColors(team);
            animation.SetController(originalEquipment.GetController());
            _health += originalEquipment.GetHealth();
            _maxHealth = _health;
            healthBar.SetHealthBar(_health,team == PlayerTeam.Player1);
            TroopsFightingManager.Instance.AddTroop(this, team);
        }

        public void GoToBuilding(Building building)
        {
            animation.Move(true);
            var speed = 2f;
            var distance = Vector3.Distance(transform.position, building.transform.position);
            var duration = distance / speed;
            var rotation = Quaternion.LookRotation(building.transform.position - transform.position);
            transform.DORotate(rotation.eulerAngles, 0.2f);
            transform.DOMove(building.transform.position, duration).OnComplete(() =>
            {
                SetEquipment(building.GetEquipmentType(), building.GetLevel());
                if (building.GetBuildingType() == BuildingType.Weapon && building.GetLinkedBuffBuilding().Count > 0)
                { 
                    animation.Move(true);
                    var availableBuilding = building.GetLinkedBuffBuilding();
                    var targetBuilding = availableBuilding[Random.Range(0, availableBuilding.Count)];
                    GoToBuilding(targetBuilding);
                }
                else
                { 
                    GoToBattle();
                }
            });

        }
        private void SetEquipment(TroopType equipment,int level)
        {
            foreach (var e in equipments)
            {
                if (equipment != e.GetEquipmentType()) continue;
                e.SetData(level-1, _team, equipment);
                _equipmentsWeHave.Add(e); 
                e.SetVisualColors(team: _team);
                if (e.ChangeAnimation()) animation.SetController(e.GetController());
                e.ShowVisual(true);
                if (e.HideOriginalBody()) originalEquipment.ShowVisual(false);
                if (e.GetEquipmentType() == TroopType.Berserker)
                {
                    _damageIncrease = e.GetDamageIncrease();
                    var increaseFactor = e.GetHealthIncrease() / 100f;
                    _health += _health * increaseFactor;
                    _maxHealth = _health;
                    healthBar.SetHealthBar(_health,_team == PlayerTeam.Player1);
                }
            }
        }
        public void GoToBattle()
        {
            _troopState = TroopState.Moving;
            StartCoroutine(SetBuffs());
            if(_team== PlayerTeam.Player1) Player.Instance.AddTroop(this);
            if(_team == PlayerTeam.Player2) Bot.Instance.AddTroop(this);
        }
        #endregion

        #region Buffs
        private IEnumerator SetBuffs()
        {
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Battle);
            yield return new WaitForSeconds(0.5f);
            foreach (var equipment in _equipmentsWeHave)
            {
                if (equipment.GetEquipmentType() == TroopType.Assassin) StartCoroutine(AssassinBuff(equipment));
            }
        }
        private IEnumerator AssassinBuff(Equipment equipment)
        {
            while (_troopState != TroopState.Death)
            {
                equipment.Hide(true); 
                gameObject.layer = 0;
                yield return new WaitForSeconds(equipment.GetAssassinHideTime());
                gameObject.layer = 6;
                equipment.Hide(false); 
                yield return new WaitForSeconds(2f);
            }
        }
        
        #endregion
        
        #region Fight
        private IEnumerator Fight()
        {
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Battle);
            while (TurnsManager.PlayState == PlayState.Battle && _troopState != TroopState.Death)
            {
                var closestTroop = GetClosestEnemy();
                if (closestTroop != null)
                {
                    var dist = Vector3.Distance(transform.position, closestTroop.transform.position);
                    if (dist < GetRange())
                    {
                        _troopState = TroopState.Attacking;
                        yield return Attack();
                        _troopState = TroopState.Moving;
                    }
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

            animation.AttackAnimation(() => { attack = true; });

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
                DamageType.Lightning => 3,
                _ => 1
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
                transform.rotation =
                    Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
                yield return null;
            }
        }

        private void MeleeAttack(Equipment weapon)
        {
            var enemy = GetClosestEnemy();
            if(enemy == null) return;
            if (Vector3.Distance(transform.position, enemy.transform.position) > weapon.GetRange()) return;
            var increaseFactor = _damageIncrease / 100f;
            var damageAmount = weapon.GetDamage().GetDamageAmount() * (1 + increaseFactor);
            enemy.TakeDamage(new Damage(damageAmount, weapon.GetDamage().GetDamageType()));
        }

        private void RangeAttack(Equipment weapon)
        {
            var target = GetClosestEnemy();
            if(target == null) return;
            var shootPoint = weapon.GetShootPoint();
            var projectile = Instantiate(weapon.GetProjectile(), shootPoint.position, shootPoint.rotation);
            
            var increaseFactor = _damageIncrease / 100f;
            var damageAmount = weapon.GetDamage().GetDamageAmount() * (1 + increaseFactor);
            
            projectile.SetProjectile(target.transform.position + Vector3.up,
                new Damage(damageAmount, weapon.GetDamage().GetDamageType()), _team);
        }

        private void AOEAttack(Equipment equipment)
        {
            var enemies = _team == PlayerTeam.Player1 ? Bot.Instance.GetTroops() : Player.Instance.GetTroops();
            var snapshot = enemies.ToList();
    
            foreach (var enemy in snapshot)
            {
                if (enemy == null) 
                    continue;

                float range = equipment.GetRange();
                if (Vector3.Distance(enemy.transform.position, transform.position) <= range)
                {
                    float increaseFactor = _damageIncrease / 100f;
                    float damageAmount = equipment.GetDamage().GetDamageAmount() * (1 + increaseFactor);
                    enemy.TakeDamage(new Damage(damageAmount, equipment.GetDamage().GetDamageType()));
                }
            }
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
        public bool IsFreezed() => _ice;
        public bool IsShocked() => _lighting;
        
        public void TakeDamage(Damage damage)
        {
            foreach (var equipment in _equipmentsWeHave)
            {
                if (equipment.GetEquipmentType() == TroopType.Shield && equipment.GetShield().IsShielded()) return;
                if (equipment.GetEquipmentType() == TroopType.Armor)
                {
                    var d = damage.GetDamageAmount();
                    var negationFactor = equipment.GetDamageNegation() / 100f;
                    d -= d * negationFactor;
                    damage = new Damage(d, damage.GetDamageType());
                }
            }
            var da = damage.GetDamageAmount();
            var difficulty = Bot.Instance.GetDifficulty();
            float p1Multiplier, p2Multiplier;

            if (difficulty >= 0f)
            {
                p1Multiplier = 1f + difficulty;         
                p2Multiplier = 1f - 0.5f * difficulty;  
            }
            else
            {
                float d = -difficulty; 
                p1Multiplier = 1f - 0.5f * d;  
                p2Multiplier = 1f + d;        
            }
            if (_team == PlayerTeam.Player1) da *= p1Multiplier;
            else da *= p2Multiplier;
            
            
            _health -= da;
            healthBar.TakeDamage(da);
            if (_health <= 0) Death();
            switch (damage.GetDamageType())
            {
                case DamageType.Ice:
                    if (!_ice) StartCoroutine(IceDamage());
                    break;
                case DamageType.Lightning:
                    if (!_lighting) StartCoroutine(LightingDamage());
                    break;
                case DamageType.Poison:
                    if (!_poisoned) StartCoroutine(PoisonDamage());
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
            var duration = 5f; 
            var tickInterval = 1f;
            var endTime = Time.time + duration;
            while (Time.time < endTime)
            {
                _health -= _maxHealth * 0.1f;
                if (_health <= 0) Death();
                healthBar.SetHealth(_health);
                yield return new WaitForSeconds(tickInterval);
            }
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
            originalEquipment.SetBodyColor(color);
            foreach (var equipment in _equipmentsWeHave)
            {
                equipment.SetBodyColor(color);
            }
        }

        public void Death(bool animate = true)
        {
            if (_troopState == TroopState.Death) return;
            StopAllCoroutines();
            foreach (var equipment in _equipmentsWeHave)
            {
                if(equipment.GetEquipmentType() == TroopType.Ghost) equipment.GetGhost().Stop();
                if (equipment.GetEquipmentType() == TroopType.Rocket) equipment.GetRocketLauncher().Stop();

            }
            _onDeath?.Invoke();
            if (animate)
            {
                deathFeedbacks?.PlayFeedbacks(); 
                animation.Death();
                Invoke(nameof(HideBody), 2f);
            }
            else
            {
                transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBounce).OnComplete(()=> Destroy(gameObject));
            }
            healthBar.gameObject.SetActive(false);
            GetComponent<Collider>().enabled = false;
            _troopState = TroopState.Death;
            rigidbody.isKinematic = true;
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