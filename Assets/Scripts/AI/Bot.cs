using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Core;
using DG.Tweening;
using Level;
using Players;
using Troops;
using UI;
using UnityEngine;
using Zombies;
using Random = UnityEngine.Random;

namespace AI
{
    public class Bot : MonoBehaviour
    {
        private string _difficultyKey = "Difficulty";
        public static Bot Instance { get; private set; }
        [SerializeField] private TroopCard[] troops;
      
        [SerializeField] private Transform player2Base;
        private readonly List<TroopCard> _troops= new List<TroopCard>();
        private List<Troop> _myTroopsOnBoard = new List<Troop>();
        private Troop _selectedTroop;
        private int _attackPoint, _movementPoint, _health = 3;
        private float _defenseWeight, _attackWeight, _killerWeight, _difficulty;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _difficulty = PlayerPrefs.GetFloat(_difficultyKey,0.5f);
            SelectRandomTroops();
            SetStrategy();
        }
        public float GetDifficulty() => _difficulty;
        private void SelectRandomTroops()
        {
            _troops.Clear();
            int selectionCount = Mathf.Min(15, troops.Length);

            var tempTroops = new List<TroopCard>(troops);
        
            for (int i = 0; i < selectionCount; i++)
            {
                int randomIndex = Random.Range(0, tempTroops.Count);
                _troops.Add(tempTroops[randomIndex]);
                tempTroops.RemoveAt(randomIndex);
            }
        }
        public (int movement, int attack) GetAbilitiesPoint()
        {
            return (_movementPoint, _attackPoint);
        }
        
        
        public void DrawDice()
        {
            var selectedDices = new List<TroopCard>();
            if (_troops.Count < 3)
            {
                selectedDices.AddRange(_troops);
            }
            else
            {
                var tempList = new List<TroopCard>(_troops);
                for (int i = 0; i < 3; i++)
                {
                    var randomIndex = Random.Range(0, tempList.Count);
                    selectedDices.Add(tempList[randomIndex]);
                    tempList.RemoveAt(randomIndex);
                }
            }
          
        }
      

        #region Ai Playing

    
      

        private IEnumerator TroopMoving(Troop troop)
        {
            var troopMoving = true;
          //  troop.GetMovement().Move(targetTile.Position, out var cost, () =>
           // {
                troopMoving = false;
          //  });
           // _movementPoint -= cost;
           yield return new WaitUntil(() => !troopMoving);
        }

        private IEnumerator TroopAttacking(Troop troop)
        {
            if(_attackPoint == 0) yield break;
            var attackFinished = false;
            _attackPoint--;
            troop.GetAnimation().Attack();
            
           /* if (targetTile.GetTroop() == null)
            {
                troop.transform.DORotate(new Vector3(0, 180, 0), 0.4f); 
                
                DOVirtual.DelayedCall(0.5f, () =>
                {
               
                    attackFinished = true;
                }, false);
                yield return new WaitWhile(() => attackFinished);
            }
            else
            {
                var r = targetTile.GetTroop().transform.position - troop.transform.position;
                troop.transform.DORotate(Quaternion.LookRotation(r).eulerAngles, 0.4f);
                troop.transform.DORotate(new Vector3(0, 180, 0), 0.4f).SetDelay(0.5f); 
                
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    targetTile.GetTroop().TakeDamage(troop.GetDamage());
                    attackFinished = true;
                }, false);
            }*/
            
           

            yield return new WaitWhile(() => attackFinished);
        }
        
        #endregion
      
        
        
        
        private void SortTroops()
        {
            var newTroopsSorting = new List<Troop>();
            var addedTroops = new HashSet<Troop>();
            
            foreach (var troop in _myTroopsOnBoard)
            {
               
            }
            foreach (var troop in _myTroopsOnBoard)
            {
                
            }
            foreach (var troop in _myTroopsOnBoard)
            {
                if (addedTroops.Add(troop))
                {
                    newTroopsSorting.Add(troop);
                }
            }

            _myTroopsOnBoard = newTroopsSorting;
        }
        private bool EnemyTroopInReach(Troop troop)
        {
            /*foreach (var movementTile in Board.Instance.GetAvailableMovementTiles(troop.transform.position,_movementPoint))
            {
                foreach (var nearbyTile in Board.Instance.GetNearbyTiles(movementTile.Position))
                {
                    if (nearbyTile.GetTroop() != null && nearbyTile.GetTroop().GetTroopTeam() == PlayerTeam.Player1)
                    {
                        tile = nearbyTile;
                        return true;
                    }
                }
            }
            tile = null;*/
            return false;
        }
       

        
       

       

        public Vector3 GetBasePosition() => player2Base.position;
        public void RemoveTroop(Troop troop)=> _myTroopsOnBoard.Remove(troop);

        public void TakeDamage()
        {
            _health--;
            if (GameManager.Instance.tutorialLevel) _health = 0;
            UIManager.Instance.playersHealth.EnemyTakesDamage();
            if(_health <= 0) GameManager.Instance.GameEnd(GameResult.Win);
        }
        public void TurnEnd()
        {
           
            _selectedTroop = null;
        }
        public int GetTroopCount()=> _myTroopsOnBoard.Count;

        public void AdjustDifficulty(bool win)
        {
            _difficulty += win ? 0.3f : -0.3f;
            _difficulty = Mathf.Clamp(_difficulty, 0, 2.5f);
            PlayerPrefs.SetFloat(_difficultyKey, _difficulty);
        }

        #region Stategy
        private void SetStrategy()
        {
             _defenseWeight = Random.Range(0f, 1f);
             _attackWeight = Random.Range(0f, 1f);
             _killerWeight = Random.Range(0f, 1f);
             
             float totalWeight = _defenseWeight + _attackWeight + _killerWeight;
            _defenseWeight /= totalWeight;
            _attackWeight /= totalWeight;
            _killerWeight /= totalWeight;
        }

        private int index;
       
       
        #endregion
    }
}
