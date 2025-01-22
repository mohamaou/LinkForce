using System;
using UnityEngine;
using UnityEditor;
using Zombies;

namespace Level
{
    // ZombiesInfo class with zombieType as string
    [Serializable]
    public class ZombiesInfo
    {
        [HideInInspector] public string zombieTypeString; 
        public ZombieType zombieType;
        public int zombiesCount;
        public float spawnTime, timeToStart;
        
        
        public void ConvertZombieType()
        {
            if (Enum.TryParse(zombieTypeString, out ZombieType result))
            {
                zombieType = result;
            }
            else
            {
                zombieType = ZombieType.NormalZombie;
            }
        }
    }
    
    [Serializable]
    public class Round
    {
        public ZombiesInfo[] zombiesInfo;
    }
    
    [Serializable]
    public class LevelData
    {
        public Round[] waves;
    }

    
    [CreateAssetMenu(fileName = "New Level", menuName = "ScriptableObjects/Level", order = 0)]
    public class Level : ScriptableObject
    {
        [SerializeField]
        public Round[] waves; // Make it public for visibility in Inspector

        [Tooltip("Select the JSON file to load wave data from")]
        public TextAsset jsonFile;

       
        public void LoadFromJson()
        {
            if (jsonFile == null)
            {
                Debug.LogWarning("No JSON file assigned. Please assign a JSON file to load data.");
                return;
            }

            // Deserialize JSON data
            LevelData data = JsonUtility.FromJson<LevelData>(jsonFile.text);
            if (data != null && data.waves != null)
            {
                waves = data.waves;
                Debug.Log("Data loaded from JSON file successfully.");

                // Log each zombie type to verify parsing
                foreach (var wave in waves)
                {
                    foreach (var zombie in wave.zombiesInfo)
                    {
                        zombie.ConvertZombieType();
                        Debug.Log($"Loaded Zombie Type: {zombie.zombieTypeString}, Count: {zombie.zombiesCount}, Spawn Time: {zombie.spawnTime}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Failed to load data from JSON file.");
            }
        }

       
        public int GetMaxRoundsCount() => waves.Length;
        
        public Round GetRound(int index)
        {
            if (index < 0 || index >= waves.Length)
                throw new IndexOutOfRangeException("Invalid wave index");
            return waves[index];
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(Level))]
    public class LevelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Reference to the Level ScriptableObject
            Level level = (Level)target;

            // Add a button to load JSON data
            if (GUILayout.Button("Load JSON Data"))
            {
                level.LoadFromJson();
                EditorUtility.SetDirty(level); // Mark the object as dirty to save changes
            }
        }
    }
    #endif
}
