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
            UIManager.Instance.startUI.SetPlayer2Card(GetSelectedBuildingCards().ToArray());
            StartCoroutine(AIPlay());
        }


        public bool IsReady() => _ready;


        private IEnumerator AIPlay()
        {
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
            var summonsTry = 0;
            while (GameManager.State == GameState.Play)
            {
                if (CoinsManager.Instance.HasCoinsToSummon(PlayerTeam.Player2) && summonsTry <= 2)
                {
                    CoinsManager.Instance.UseCoins(PlayerTeam.Player2);
                    if (!SummonRandomBuilding()) summonsTry++;
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
                    var l = troopBuilding.CreateLink();
                    l.SetLink(PlayerTeam.Player2);
                    l.ShowLink(troopBuilding.transform.position, weaponBuilding.transform.position);
                    l.SetBuildings(troopBuilding, weaponBuilding);

                    if (ValidateLink(troopBuilding, weaponBuilding))
                        LinkBuildings(troopBuilding, weaponBuilding, l);
                    else
                        Destroy(l.gameObject);

                    yield return new WaitForSeconds(1f);
                }
            }

            foreach (var weaponBuilding in weaponBuildings)
            {
                foreach (var buffBuilding in buffBuildings)
                {
                    var l = weaponBuilding.CreateLink();
                    l.SetLink(PlayerTeam.Player2);
                    l.ShowLink(weaponBuilding.transform.position, buffBuilding.transform.position);
                    l.SetBuildings(weaponBuilding, buffBuilding);

                    if (ValidateLink(weaponBuilding, buffBuilding))
                        LinkBuildings(weaponBuilding, buffBuilding, l);
                    else
                        Destroy(l.gameObject);
                }
            }
        }


        private IEnumerator TryToMerge()
        {
            if (BuildingsOnBoard.Count == 0) 
                yield break;
            
            var outerSnapshot = new List<Building>(BuildingsOnBoard);

            foreach (var building in outerSnapshot)
            {
                if (!BuildingsOnBoard.Contains(building))
                    continue;
                
                var innerSnapshot = new List<Building>(BuildingsOnBoard);

                foreach (var b in innerSnapshot)
                {
                    if (!BuildingsOnBoard.Contains(b))
                        continue;

                    if (b == building)
                        continue;

                    var mergeDone = false;

                    if (ValidateMerge(b, building))
                    {
                        MergeBuildings(b, building, () => mergeDone = true);
                        yield return new WaitUntil(() => mergeDone);
                        yield return new WaitForSeconds(1f);
                    }
                }
            }
        }

    }
}