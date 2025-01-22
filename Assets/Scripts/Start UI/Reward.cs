using System.IO;
using Start_UI;
using UnityEngine;


[System.Serializable]
public class RewardData
{
    public bool full;
    public RewardLoot[] rewards;
}

namespace Start_UI
{
    [CreateAssetMenu(fileName = "NewReward", menuName = "ScriptableObjects/Reward", order = 1)]
    public class Reward : ScriptableObject
    {
        [SerializeField] private string id;
        public bool full, debugging;
        [SerializeField] private RewardLoot[] rewards;

        public bool IsFull() => full;
        public RewardLoot[] GetReward() => rewards;

        public void SetReward(RewardLoot[] r)
        {
            rewards = r;
            SaveData();
        }

        // Save the data to a file
        public void SaveData()
        {
            var data = new RewardData
            {
                full = full,
                rewards = rewards
            };

            string json = JsonUtility.ToJson(data);
            string path = Path.Combine(Application.persistentDataPath, name + ".json");
            File.WriteAllText(path, json);
        }
        
        public void LoadData()
        {
            if (debugging) return;
            if (PlayerPrefs.GetInt("Fists Load" +id , 1) == 1)
            {
                PlayerPrefs.SetInt("Fists Load" + id, 0);
                rewards = null;
                full = false;
                SaveData();
            }
            string path = Path.Combine(Application.persistentDataPath, name + ".json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                RewardData data = JsonUtility.FromJson<RewardData>(json);

                full = data.full;
                rewards = data.rewards;
            }
        }
    }
}