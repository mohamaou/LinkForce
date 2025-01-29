using System.Collections.Generic;
using Cards;
using Troops;
using UI;
using UnityEngine;


namespace AI
{
    public class Bot : MonoBehaviour
    {
        private string _difficultyKey = "Difficulty";
        public static Bot Instance { get; private set; }
        [SerializeField] private BuildingCard[] building;
        [SerializeField] [Range(1,5)] private int minimumTroopBuildings = 1;
        private readonly List<BuildingCard> _buildings= new List<BuildingCard>();
        private readonly List<Troop> _myTroops = new List<Troop>();
        

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetDeck();
            UIManager.Instance.startUI.SetPlayer2Card(_buildings.ToArray());
        }

        private void SetDeck()
        {
            var selectedIndices = new HashSet<int>();
            var troopBuildings = new List<int>();

            for (int i = 0; i < building.Length; i++)
            {
                if (building[i].GetBuildingType() == BuildingType.Troops)
                {
                    troopBuildings.Add(i);
                }
            }

            int requiredTroops = Mathf.Min(minimumTroopBuildings, troopBuildings.Count);

            while (selectedIndices.Count < requiredTroops)
            {
                int randomTroopIndex = troopBuildings[Random.Range(0, troopBuildings.Count)];
                if (!selectedIndices.Contains(randomTroopIndex))
                {
                    selectedIndices.Add(randomTroopIndex);
                    _buildings.Add(building[randomTroopIndex]);
                }
            }

            while (selectedIndices.Count < 5)
            {
                int randomIndex = Random.Range(0, building.Length);
                if (!selectedIndices.Contains(randomIndex))
                {
                    selectedIndices.Add(randomIndex);
                    _buildings.Add(building[randomIndex]);
                }
            }
        }



        public void AddTroop(Troop troop) => _myTroops.Add(troop);
        public List<Troop> GetTroops() => _myTroops;
    }
}
