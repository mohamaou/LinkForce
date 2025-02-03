using System.Collections.Generic;
using System.Linq;
using Cards;
using Core;
using DG.Tweening;
using Models;
using Troops;
using UI;
using UnityEngine;

namespace Players
{
    public enum PlayerTeam
    {
        Player1,
        Player2
    }

    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private BuildingCard[] buildingCards;
        [SerializeField] private PlayerTeam team;
        private readonly List<BuildingCard> _selectedBuilding = new();
        private readonly List<Building> _buildingsOnBoard = new();
        private readonly List<Troop> _myTroops = new();
        private List<Link> _myLinks = new();


        protected void SetDeck()
        {
            if (team == PlayerTeam.Player1)
            {
                foreach (var card in buildingCards)
                {
                    if (card.IsSelected()) _selectedBuilding.Add(card);
                }

                UIManager.Instance.startUI.SetPlayer1Card(_selectedBuilding.ToArray());
            }
            else
            {
                var selectedIndices = new HashSet<int>();
                var troopBuildings = new List<int>();

                for (int i = 0; i < buildingCards.Length; i++)
                {
                    if (buildingCards[i].GetBuildingType() == BuildingType.Troops)
                    {
                        troopBuildings.Add(i);
                    }
                }

                var requiredTroops = Mathf.Min(2, troopBuildings.Count);

                while (selectedIndices.Count < requiredTroops)
                {
                    int randomTroopIndex = troopBuildings[Random.Range(0, troopBuildings.Count)];
                    if (!selectedIndices.Contains(randomTroopIndex))
                    {
                        selectedIndices.Add(randomTroopIndex);
                        _selectedBuilding.Add(buildingCards[randomTroopIndex]);
                    }
                }

                while (selectedIndices.Count < 5)
                {
                    int randomIndex = Random.Range(0, buildingCards.Length);
                    if (!selectedIndices.Contains(randomIndex))
                    {
                        selectedIndices.Add(randomIndex);
                        _selectedBuilding.Add(buildingCards[randomIndex]);
                    }
                }

                UIManager.Instance.startUI.SetPlayer2Card(_selectedBuilding.ToArray());
            }
        }

        protected List<BuildingCard> GetSelectedBuildingCards() => _selectedBuilding;

        #region public Variable

        public void AddTroop(Troop troop)
        {
            _myTroops.Add(troop);
            troop.SetDeathEvent(() => _myTroops.Remove(troop));
        }

        public List<Troop> GetTroops() => _myTroops;
        protected List<Building> BuildingsOnBoard => _buildingsOnBoard;
        protected List<Link> MyLinks() => _myLinks;

        #endregion

        #region Summon Building

        protected bool SummonRandomBuilding()
        {
            System.Random random = new System.Random();
            var selectedBuilding = _selectedBuilding.OrderBy(b => random.Next()).ToList();
            if (!TroopBuildingAvailable())
            {
                var troopBuildingListList = _selectedBuilding.Where(b => b.GetBuildingType() == BuildingType.Troops)
                    .OrderBy(b => random.Next()).ToList();
                if (troopBuildingListList.Count > 0) selectedBuilding = troopBuildingListList;
            }

            foreach (var building in selectedBuilding)
            {
                var summonPos = Board.Instance.GetRandomBoardPoint(team, building.GetBuildingType());
                var summonRot = team == PlayerTeam.Player1 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
                if (summonPos != Vector3.zero)
                {
                    var b = Instantiate(building.GetBuilding(), summonPos, summonRot, transform);
                    b.Set(team);
                    b.transform.parent = Board.Instance.GetBoard(team);
                    _buildingsOnBoard.Add(b);
                    return true;
                }
            }

            return false;
        }

        protected bool SummonBuilding(int index)
        {
            var building = _selectedBuilding[index];
            var summonPos = Board.Instance.GetRandomBoardPoint(team, building.GetBuildingType());
            var summonRot = team == PlayerTeam.Player1 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
            if (summonPos != Vector3.zero)
            {
                var b = Instantiate(building.GetBuilding(), summonPos, summonRot, transform);
                b.Set(team);
                b.transform.parent = Board.Instance.GetBoard(team);
                _buildingsOnBoard.Add(b);
                return true;
            }
            return false;
        }

        private bool TroopBuildingAvailable()
        {
            foreach (var building in _buildingsOnBoard)
            {
                if (building.GetBuildingType() == BuildingType.Troops) return true;
            }

            return false;
        }

        #endregion

        #region Links Manage

        protected bool ValidateLink(Building building1, Building building2)
        {
            if (building1 == building2) return false;
            var type1 = building1.GetBuildingType();
            var type2 = building2.GetBuildingType();
            var availableLinksToBuffs1 = building1.GetLinksToTroops() == 0
                ? 1 - building1.GetLinksToBuffs()
                : building1.GetLinksToBuffs() - building1.GetLinksToTroops();
            var availableLinksToBuffs2 = building2.GetLinksToTroops() == 0
                ? 1 - building2.GetLinksToBuffs()
                : building2.GetLinksToBuffs() - building2.GetLinksToTroops();
            var linksCount1 = building1.GetLinksCount();
            var linksCount2 = building2.GetLinksCount();

            var isTroopsBuff = (type1 == BuildingType.Troops && type2 == BuildingType.Buff) ||
                               (type1 == BuildingType.Buff && type2 == BuildingType.Troops);
            var isTroopsTroops = type1 == BuildingType.Troops && type2 == BuildingType.Troops;
            var sameType = type1 == type2;

            var validTypes = !isTroopsBuff && !isTroopsTroops;
            var noLinksToBuff = (type1 == BuildingType.Weapon && type2 == BuildingType.Buff &&
                                 availableLinksToBuffs1 == 0) || (type2 == BuildingType.Weapon &&
                                                                  type1 == BuildingType.Buff &&
                                                                  availableLinksToBuffs2 == 0);
            var troopsAlreadyLinked = (type1 == BuildingType.Troops && linksCount1 > 0) ||
                                      (type2 == BuildingType.Troops && linksCount2 > 0);

            var canLink = !sameType && !noLinksToBuff && !troopsAlreadyLinked;
            return validTypes && canLink;
        }

        protected bool ValidateMerge(Building building1, Building building2)
        {
            if (building1 == null || building2 == null) return false;
            if (building1 == building2) return false;
            var type1 = building1.GetBuildingType();
            var type2 = building2.GetBuildingType();

            var level1 = building1.GetLevel();
            var level2 = building2.GetLevel();

            var sameType = type1 == type2;
            var sameId = building1.GetEquipmentType() == building2.GetEquipmentType();
            var sameLevel = level1 == level2;

            var canMerge = sameId && sameLevel && sameType;
            return canMerge;
        }

        protected void LinkBuildings(Building building, Building targetBuilding, Link link)
        {
            link.SetBuildings(building, targetBuilding);
            targetBuilding.SetBuildingLink(link);
            building.SetBuildingLink(link);
            _myLinks.Add(link);

            // Increment Weapon to Troops links number
            if (building.GetBuildingType() == BuildingType.Weapon &&
                targetBuilding.GetBuildingType() == BuildingType.Troops) building.IncrementLinksToTroops();
            else if (building.GetBuildingType() == BuildingType.Troops &&
                     targetBuilding.GetBuildingType() == BuildingType.Weapon)
                targetBuilding.IncrementLinksToTroops();

            // Increment Weapon to Buffs links number
            if (building.GetBuildingType() == BuildingType.Weapon &&
                targetBuilding.GetBuildingType() == BuildingType.Buff) building.IncrementLinksToBuffs();
            else if (building.GetBuildingType() == BuildingType.Buff &&
                     targetBuilding.GetBuildingType() == BuildingType.Weapon)
                targetBuilding.IncrementLinksToBuffs();
        }

        protected void MergeBuildings(Building target, Building source, System.Action mergeDone)
        {
            source.transform.DOMove(target.transform.position, .3f).OnComplete(() =>
            {
                target.RunGFX();
                target.IncrementLevel();
                _buildingsOnBoard.Remove(source);

                if (source.GetBuildingType() == BuildingType.Troops && source.GetMyLinks().Count > 0 &&
                    target.GetMyLinks().Count > 0)
                {
                    var sourceLinks = source.GetMyLinks();
                    foreach (var link in sourceLinks)
                    {
                        var otherBuilding = link.GetLinkedBuilding(source);
                        otherBuilding.RemoveLink(link);
                        otherBuilding.SetActive(false);
                        otherBuilding.SetLinksToTroops(otherBuilding.GetLinksToTroops() - 1);
                        _myLinks.Remove(link);
                        Destroy(link.gameObject);
                    }
                }
                else
                {
                    var sourceLinks = source.GetMyLinks();
                    foreach (var link in sourceLinks)
                    {
                        var otherBuilding = link.GetLinkedBuilding(source);
                        otherBuilding.RemoveLink(link);

                        var newLink = otherBuilding.CreateLink();
                        newLink.SetLink(team);
                        newLink.SetLink(team);

                        newLink.SetBuildings(otherBuilding, target);
                        target.SetBuildingLink(newLink);
                        otherBuilding.SetBuildingLink(newLink);

                        newLink.ShowLink(target.transform.position, otherBuilding.transform.position);
                        _myLinks.Remove(link);
                        _myLinks.Add(newLink);
                        Destroy(link.gameObject);
                    }

                    target.SetLinksToTroops(target.GetLinksToTroops() + source.GetLinksToTroops());
                    target.SetLinksToBuffs(target.GetLinksToBuffs() + source.GetLinksToBuffs());
                }

                Board.Instance.BuildingMerged(target.transform.position, team);
                CoinsManager.Instance.AddMergeReward(team);
                Destroy(source.gameObject);
                mergeDone?.Invoke();
            });
            Board.Instance.ClearTile(source.transform.position);
        }

        #endregion
    }
}