using System;
using UnityEngine;
using UnityEditor;

namespace Core
{
    public enum AgeType
    {
        Economy, Military 
    }
    [Serializable]
    public class AgeData
    {
        [SerializeField] private Age age;
        [SerializeField] private AgeType ageType;
        [SerializeField] private Sprite ageSprite;
        [SerializeField] private int upgradeCost;
        [SerializeField] private float costReduction;
        [SerializeField] private float healthAndDamageBoost;

        public Age GetAge() => age;
        public int GetUpgradeCost() => upgradeCost;
        public Sprite GetAgeSprite() => ageSprite;
        public AgeType GetAgeType() => ageType;
        public float GetCostReduction() => costReduction;
        public float GetHealthAndDamageBoost() => healthAndDamageBoost;
    }

    [Serializable]
    public struct AgesOption
    {
        public string label;
        public AgeData[] age;
    }

    public class AgeManager : MonoBehaviour
    {
        public static AgeManager Instance;
        [SerializeField] private AgesOption[] ageOptions;

        private void Awake()
        {
            Instance = this;
        }

        public Age GetStartingAge() => ageOptions[0].age[0].GetAge();

        public AgeData GetAgeData(Age age)
        {
            for (int i = 0; i < ageOptions.Length; i++)
            {
                for (int j = 0; j < ageOptions[i].age.Length; j++)
                {
                    if(ageOptions[i].age[j].GetAge() == age) return ageOptions[i].age[j];
                }
            }
            return null;
        }

        public AgeData[] GetNexAgeData(Age age)
        {
            var nexAge = GetRandomNextAge(age);
            for (int i = 0; i < ageOptions.Length; i++)
            {
                for (int j = 0; j < ageOptions[i].age.Length; j++)
                {
                    if(ageOptions[i].age[j].GetAge() == nexAge) return ageOptions[i].age;
                }
            }
            return null;
        }

        public Age GetRandomNextAge(Age age)
        {
            for (int i = 0; i < ageOptions.Length; i++)
            {
                for (int j = 0; j < ageOptions[i].age.Length; j++)
                {
                    if (ageOptions[i].age[j].GetAge() == age)
                    {
                        if (i + 1 < ageOptions.Length)
                        {
                            AgeData[] nextAges = ageOptions[i+1].age;
                            int randIndex = UnityEngine.Random.Range(0, nextAges.Length);
                            return nextAges[randIndex].GetAge();
                        }
                        else
                        {
                            return Age.Null;
                        }
                    }
                }
            }
            return Age.Null;
        }

        public int GetAgeUpgradeCost(Age age)
        {
            var ageData = GetAgeData(age);
            if(ageData == null) return 100000;
            return ageData.GetUpgradeCost();
        }
        public int GetNextAgeUpgradeCost(Age age)
        {
            var ageData = GetAgeData(age);
            for (int i = 0; i < ageOptions.Length; i++)
            {
                for (int j = 0; j < ageOptions[i].age.Length; j++)
                {
                    if (ageOptions[i].age[j].GetAge() == age)
                    {
                        if (i + 1 < ageOptions.Length)
                        {
                            ageData = ageOptions[i + 1].age[j];
                        }
                        else
                        {
                            return 100000; 
                        }
                        break;
                    }
                }
            }
            return ageData?.GetUpgradeCost() ?? 100000;
        }

        public Age GetNexAge(Age age, int index)
        {
            for (int i = 0; i < ageOptions.Length; i++)
            {
                for (int j = 0; j < ageOptions[i].age.Length; j++)
                {
                    if (ageOptions[i].age[j].GetAge() == age)
                    {
                        if (i + 1 < ageOptions.Length)
                        {
                            AgeData[] nextAges = ageOptions[i+1].age;
                            return nextAges[index].GetAge();
                        }
                        else
                        {
                            return Age.Null;
                        }
                    }
                }
            }
            return Age.Null;
        }
    }

#if UNITY_EDITOR

[CustomEditor(typeof(Core.AgeManager))]
public class AgeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Get the ageOptions property
        SerializedProperty ageOptions = serializedObject.FindProperty("ageOptions");

        // Set full-width layout
        EditorGUIUtility.labelWidth = 150;
        EditorGUILayout.LabelField("Age Options", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Loop through each age option
        for (int i = 0; i < ageOptions.arraySize; i++)
        {
            SerializedProperty option = ageOptions.GetArrayElementAtIndex(i);
            SerializedProperty ages = option.FindPropertyRelative("age");

            EditorGUILayout.LabelField($"Age Option {i + 1}", EditorStyles.boldLabel);

            // Loop through each age in the option
            for (int j = 0; j < ages.arraySize; j++)
            {
                SerializedProperty ageData = ages.GetArrayElementAtIndex(j);
                SerializedProperty age = ageData.FindPropertyRelative("age");
                SerializedProperty ageType = ageData.FindPropertyRelative("ageType"); // New field
                SerializedProperty sprite = ageData.FindPropertyRelative("ageSprite");
                SerializedProperty cost = ageData.FindPropertyRelative("upgradeCost");
                SerializedProperty costReduction = ageData.FindPropertyRelative("costReduction");
                SerializedProperty healthBoost = ageData.FindPropertyRelative("healthAndDamageBoost");

                // Boxed layout for readability
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Editable Age enum
                EditorGUILayout.PropertyField(age, new GUIContent("Age"));

                // New AgeType enum field
                EditorGUILayout.PropertyField(ageType, new GUIContent("Age Type"));

                // Other editable fields
                EditorGUILayout.PropertyField(sprite, new GUIContent("Age Sprite"));
                cost.intValue = EditorGUILayout.IntField("Upgrade Cost", cost.intValue);
                costReduction.floatValue = EditorGUILayout.FloatField("Cost Reduction", costReduction.floatValue);
                healthBoost.floatValue = EditorGUILayout.FloatField("Health & Damage Boost", healthBoost.floatValue);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.Space(10);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
}
