using System;
using Troops;
using UnityEditor;
using UnityEngine;


namespace Cards
{
    [Serializable]
    public class Melee
    {
        [SerializeField] private float[] damage;

        public float GetDamage(int level) => damage[level];
    }

    [Serializable]
    public class Range
    {
        [SerializeField] private float[] damage;
        [SerializeField] private float range;
        
        public float GetDamage(int level) => damage[level];
        public float GetRange() => range;
    }
    [Serializable]
    public class Berserker
    {
        [SerializeField] private float[] damageIncreased, healthIncreased;
        public float GetDamageIncreased(int level) => damageIncreased[level];
        public float GetHealthIncreased(int level) => healthIncreased[level];
    }

    [Serializable]
    public class Ghost
    {
        [SerializeField] private float[] ghostDamage;
        public float GetDamage(int level) => ghostDamage[level];
    }

    [Serializable]
    public class Rocket
    {
        [SerializeField] private float[] rocketDamage;
        [SerializeField] private float shootTime;
        public float GetDamage(int level) => rocketDamage[level];
        public float GetRange() => shootTime;
    }

    [Serializable]
    public class Armor
    {
        [SerializeField] [Range(0,100)] private float[] damageNegation;
        
        public float GetDamageNegation(int level) => damageNegation[level];
    }

    [Serializable]
    public class Shield
    {
        [SerializeField] private int[] shieldCount;
        public int GetShieldCount(int level) => shieldCount[level];
    }

    [Serializable]
    public class Assassin
    {
        [SerializeField] private float[] hideTime;
        [SerializeField] private float[] coolDownTime;
        public float GetHideTime(int level) => hideTime[level];
        public float GetCoolDownTime(int level) => coolDownTime[level];
    }

    [Serializable]
    public class TroopStat
    {
        [SerializeField] private int[] damage, health;
        [SerializeField] private float speed;
    }
    
    
    
    [CreateAssetMenu(fileName = "NewCard", menuName = "ScriptableObjects/Monster Card", order = 1)]
    public class BuildingCard : ScriptableObject
    {
        [SerializeField] private new string name;
        [SerializeField] private BuildingType buildingType;
        [SerializeField] private TroopType troopType;
        [SerializeField] private Sprite cardSprite;
        [SerializeField] private Building building;
        [SerializeField] private bool defaultTroop;
        [SerializeField] private string description;
        
        public Melee melee;
        public Range range;
        public Berserker berserker;
        public Ghost ghost;
        public Rocket rocket;
        public Shield shield;
        public Armor armor; 
        public Assassin assassin;
        public TroopStat troop;
        private Action _onCardAddedEvent;



        public TroopType GetTroopType() => troopType;
        public Sprite GetSprite() => cardSprite;
        public string GetTroopName() => name;
        public bool IsLocked()
        {
            var key = $"IsLocked_{name}";
            return PlayerPrefs.GetInt(key, defaultTroop ? 0 : 1) == 1;
        }
        public bool IsSelected()
        {
            var key = $"IsSelected_{name}";
            return PlayerPrefs.GetInt(key, defaultTroop ? 1 : 0) == 1;
        }
        public string GetDescription() => description;
        public Building GetBuilding() => building;
        public BuildingType GetBuildingType() => buildingType;
        
        

        #region Card Level
        public void SetCardChangedEvent(System.Action evento) => _onCardAddedEvent += evento;
        public int GetCardLevel()
        {
            var key = $"CardLevel_{name}";
            return PlayerPrefs.GetInt(key, 1);
        }
        public int AvailableCard()
        {
            var key = $"AvailableCard{name}";
            return PlayerPrefs.GetInt(key, 0);
        }
        public void AddCard(int card)
        {
            if (IsLocked())
            {
                var keyLock = $"IsLocked_{name}";
                PlayerPrefs.SetInt(keyLock, 0);
            }
            var key = $"AvailableCard{name}";
            var availableCard = AvailableCard();
            PlayerPrefs.SetInt(key, card + availableCard);
            _onCardAddedEvent?.Invoke();
        }
        public void LevelUp(int cardsCost)
        {
            AddCard(-cardsCost);
            var key = $"CardLevel_{name}";
            var currentLevel = GetCardLevel() +1;
            PlayerPrefs.SetInt(key, currentLevel);
            _onCardAddedEvent?.Invoke();
        }
        #endregion

        public void Select() => PlayerPrefs.SetInt($"IsSelected_{name}", 1);
        public void Deselect() => PlayerPrefs.SetInt($"IsSelected_{name}", 0);
    }
    

#if UNITY_EDITOR
[CustomEditor(typeof(BuildingCard))]
public class BuildingCardEditor : Editor
{
    private SerializedProperty nameProp;
    private SerializedProperty buildingTypeProp;
    private SerializedProperty troopTypeProp;
    private SerializedProperty cardSpriteProp;
    private SerializedProperty troopProp;
    private SerializedProperty defaultTroopProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty meleeProp;
    private SerializedProperty rangeProp;
    private SerializedProperty berserkerProp;
    private SerializedProperty ghostProp;
    private SerializedProperty rocketProp;
    private SerializedProperty shieldProp;
    private SerializedProperty assassinProp; 
    private SerializedProperty armorProp; 
    private SerializedProperty troopStatsProp;

    private void OnEnable()
    {
        nameProp          = serializedObject.FindProperty("name");
        buildingTypeProp  = serializedObject.FindProperty("buildingType");
        troopTypeProp     = serializedObject.FindProperty("troopType");
        cardSpriteProp    = serializedObject.FindProperty("cardSprite");
        troopProp         = serializedObject.FindProperty("building");
        defaultTroopProp  = serializedObject.FindProperty("defaultTroop");
        descriptionProp   = serializedObject.FindProperty("description");

        meleeProp         = serializedObject.FindProperty("melee");
        rangeProp         = serializedObject.FindProperty("range");
        berserkerProp     = serializedObject.FindProperty("berserker");
        ghostProp         = serializedObject.FindProperty("ghost");
        rocketProp        = serializedObject.FindProperty("rocket");
        shieldProp        = serializedObject.FindProperty("shield");
        assassinProp      = serializedObject.FindProperty("assassin"); 
        armorProp         = serializedObject.FindProperty("armor");
        troopStatsProp    = serializedObject.FindProperty("troop");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(nameProp,         new GUIContent("Name"));
        EditorGUILayout.PropertyField(buildingTypeProp, new GUIContent("Building Type"));
        EditorGUILayout.PropertyField(troopTypeProp,    new GUIContent("Troop Type"));
        EditorGUILayout.PropertyField(cardSpriteProp,   new GUIContent("Card Sprite"));
        EditorGUILayout.PropertyField(troopProp,        new GUIContent("Building"));
        EditorGUILayout.PropertyField(defaultTroopProp, new GUIContent("Default Troop"));
        EditorGUILayout.PropertyField(descriptionProp,  new GUIContent("Description"));
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        BuildingCard buildingCard = (BuildingCard)target;
        switch (buildingCard.GetTroopType())
        {
            case TroopType.Sword:
            case TroopType.Axe:
                EditorGUILayout.PropertyField(meleeProp, true);
                break;
            case TroopType.Bow:
            case TroopType.Electric:
            case TroopType.Ice:
            case TroopType.Poison:
                EditorGUILayout.PropertyField(rangeProp, true);
                break;
            case TroopType.Berserker:
                EditorGUILayout.PropertyField(berserkerProp, true);
                break;
            case TroopType.Ghost:
                EditorGUILayout.PropertyField(ghostProp, true);
                break;
            case TroopType.Rocket:
                EditorGUILayout.PropertyField(rocketProp, true);
                break;
            case TroopType.Shield:
                EditorGUILayout.PropertyField(shieldProp, true);
                break;
            case TroopType.Assassin: 
                EditorGUILayout.PropertyField(assassinProp, true);
                break;
            case TroopType.Armor: 
                EditorGUILayout.PropertyField(armorProp, true);
                break;
            case TroopType.Zombie:
            case TroopType.Human:
            case TroopType.Skeleton:
            case TroopType.Goblin: 
                EditorGUILayout.PropertyField(troopStatsProp, true);
                break;
            default:
                EditorGUILayout.HelpBox("No stats to display for this troop type.", MessageType.Info);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif


}
