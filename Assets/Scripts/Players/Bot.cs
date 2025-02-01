using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Troops;
using UI;
using UnityEngine;

namespace Players
{
    public class Bot : PlayerManager
    {
        private string _difficultyKey = "Difficulty";
        public static Bot Instance { get; private set; }
        private bool _ready;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetDeck();
            UIManager.Instance.startUI.SetPlayer2Card(GetSelectedBuildingCards() .ToArray());
            StartCoroutine(AIPlay());
        }
        
        
       
        public bool IsReady() => _ready;


        private IEnumerator AIPlay()
        {
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
            while (GameManager.State  == GameState.Play)
            {
                if (CoinsManager.HasCoinsToSummon(PlayerTeam.Player2))
                {
                    CoinsManager.Instance.UseCoins(PlayerTeam.Player2);
                    SummonRandomBuilding();
                    yield return new WaitForSeconds(Random.Range(0.2f, .5f));
                }
                else
                {
                    yield return TryToMerge();
                    yield return TryLinkBuildings();
                    _ready = true;
                }
                yield return new WaitForSeconds(Random.Range(0.2f, 1f));
            }
        }

        private IEnumerator TryLinkBuildings()
        {
            if (BuildingsOnBoard.Count == 0)
                yield break;

            System.Random random = new System.Random();

            var troopBuildings = BuildingsOnBoard
                .Where(b => b.GetBuildingType() == BuildingType.Troops)
                .OrderBy(b => random.Next())
                .ToList();

            var weaponBuildings = BuildingsOnBoard
                .Where(b => b.GetBuildingType() == BuildingType.Weapon)
                .OrderBy(b => random.Next())
                .ToList();

            var buffBuildings = BuildingsOnBoard
                .Where(b => b.GetBuildingType() == BuildingType.Buff)
                .OrderBy(b => random.Next())
                .ToList();

            foreach (var troopBuilding in troopBuildings)
            {
                foreach (var weaponBuilding in weaponBuildings)
                {
                    if (TryToLinkBuildings(troopBuilding, weaponBuilding))
                        yield return new WaitForSeconds(1f);
                }
            }

            foreach (var weaponBuilding in weaponBuildings)
            {
                foreach (var buffBuilding in buffBuildings)
                {
                    if (TryToLinkBuildings(weaponBuilding, buffBuilding))
                        yield return new WaitForSeconds(1f);
                }
            }
        }


        private IEnumerator TryToMerge()
        {
            if (BuildingsOnBoard.Count == 0) yield break;
            foreach (var building in BuildingsOnBoard.ToList())
            {
                foreach (var b in BuildingsOnBoard.ToList())
                {
                    if (b != building)
                        if (TryMergeBuildings(b, building))
                            yield return new WaitForSeconds(1f);
                }
            }
        }
    }
}