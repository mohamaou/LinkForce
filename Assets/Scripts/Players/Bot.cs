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
            if(!GameManager.Instance.tutorialLevel)StartCoroutine(AIPlay());
            else StartCoroutine(AITutorial());
        }


        public bool IsReady() => _ready;


        private IEnumerator AIPlay()
        {
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
            while (GameManager.State == GameState.Play)
            {
                _ready = false;   
                yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Summon);
                yield return new WaitForSeconds(Random.Range(1.5f, 2f));
                while (!_ready)
                {
                    var summonsTry = 0;
                    while (CoinsManager.Instance.HasCoinsToSummon(PlayerTeam.Player2) && summonsTry <= 2)
                    {
                        CoinsManager.Instance.UseCoins(PlayerTeam.Player2);
                        if (!SummonRandomBuilding()) summonsTry++;
                        yield return new WaitForSeconds(Random.Range(0.2f, 0.7f));
                    }

                    yield return TryLinkBuildings();
                    yield return TryToMerge();
                
                    if(!CoinsManager.Instance.HasCoinsToSummon(PlayerTeam.Player2) || summonsTry > 2) _ready = true;
                    yield return new WaitForSeconds(Random.Range(0.2f, .5f));
                }

                yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Battle);
            }
        }

        private IEnumerator AITutorial()
        {
            yield return new WaitUntil(() => GameManager.State == GameState.Play);

            _ready = false;   
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Summon);
            yield return new WaitForSeconds(Random.Range(1.5f, 2f));

            //Round 1
            SummonBuilding(0);
            yield return new WaitForSeconds(Random.Range(0.2f, 1.7f));
            SummonBuilding(1);
            yield return TryLinkBuildings();
            yield return TryToMerge();
            _ready = true;
                
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Battle);
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Summon);
            
            //Round2
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
            
            foreach (var troopBuilding in troopBuildings)
            {
                var weaponBuildings = BuildingsOnBoard
                    .Where(b => b.GetBuildingType() == BuildingType.Weapon)
                    .OrderBy(b => random.Next())
                    .ToList();
                foreach (var weaponBuilding in weaponBuildings)
                {
                    if (ValidateLink(troopBuilding, weaponBuilding))
                    {
                        var l = troopBuilding.CreateLink();
                        l.SetLink(PlayerTeam.Player2);
                        l.ShowLink(troopBuilding.transform.position, weaponBuilding.transform.position);
                        l.SetBuildings(troopBuilding, weaponBuilding);
                        LinkBuildings(troopBuilding, weaponBuilding, l);
                        yield return new WaitForSeconds(.7f);
                    }
                }
            }
            
            foreach (var weaponBuilding in BuildingsOnBoard.Where(b => b.GetBuildingType() == BuildingType.Weapon))
            {
                var buffBuildings = BuildingsOnBoard
                    .Where(b => b.GetBuildingType() == BuildingType.Buff)
                    .OrderBy(b => random.Next())
                    .ToList();
                foreach (var buffBuilding in buffBuildings)
                {
                    if (ValidateLink(weaponBuilding, buffBuilding))
                    {
                        var l = weaponBuilding.CreateLink();
                        l.SetLink(PlayerTeam.Player2);
                        l.ShowLink(weaponBuilding.transform.position, buffBuilding.transform.position);
                        l.SetBuildings(weaponBuilding, buffBuilding);
                        LinkBuildings(weaponBuilding, buffBuilding, l);
                    }
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
                        yield return new WaitForSeconds(.7f);
                    }
                }
            }
        }

    }
}