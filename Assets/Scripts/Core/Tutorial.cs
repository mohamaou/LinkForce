using System;
using System.Collections;
using System.Linq;
using Level;
using Players;
using Troops;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zombies;

namespace Core
{
    public class Tutorial : MonoBehaviour
    {

        public static Tutorial Instance { get; private set; }
        [SerializeField] private Button hand;
        [SerializeField] private Image mask;
        [SerializeField] private GameObject[] hintsPanels;
        [SerializeField] private Button endTurnButton;
        private int _diceIndex;
        private Transform _card;
        private Camera _cam;
        private bool _cardSelected;
        private Troop _troop;
        private bool _diceSummoned;
        private bool _secondSummon;
        private bool _troopAttacked;
        private System.Action _panelClosed;
        

        private void Awake()
        {
            Instance = this;
            _cam = Camera.main;
        }

        public void CardSelected() => _cardSelected = true;
        public void SetTroop(Troop troop) => _troop = troop;

        public void TroopSelected(Troop troop)
        {
            _troop = troop;
           // var tilePosition = Board.Instance.GetClosestAvailableEdgeTile(troop, 50).Position;
            //hand.transform.position = _cam.WorldToScreenPoint(tilePosition);
        }

        public void DiceSummoned(Action closed)
        {
            _panelClosed += closed;
            _diceSummoned = true;
        }
        public bool IsSecondSummoned() => _secondSummon;

        public void SetAttackButton(EventTrigger button)
        {
            hand.onClick.RemoveAllListeners();
            hand.onClick.AddListener(() =>
            {
                foreach (var entry in button.triggers)
                    if (entry.eventID == EventTriggerType.PointerClick)
                        entry.callback.Invoke(new BaseEventData(EventSystem.current));
            });
            hand.transform.position = button.transform.position;
        }
        public void SetAttackButton(Button button)
        {
            hand.onClick.RemoveAllListeners();
            hand.transform.position = button.transform.position;
            hand.onClick.AddListener(() => {
            {
                button.onClick.Invoke();
                _troopAttacked = true;
            } });
            
        }

        private IEnumerator Start()
        {
            _diceIndex = 0;
            mask.enabled = false;
            hand.gameObject.SetActive(false);
            foreach (var panel in hintsPanels)
            {
                panel.SetActive(false);
            }
            yield return new WaitUntil(() => TurnsManager.TurnStats == TurnStats.TroopSelection); 
            
            hintsPanels[0].SetActive(true);
            yield return new WaitUntil(() => _cardSelected); 
            
            _cardSelected = false;
            hintsPanels[0].SetActive(false);
            hand.gameObject.SetActive(true);
            yield return new WaitUntil(() => _troop != null);
            
            mask.enabled = true;
            hand.gameObject.SetActive(true);
            hand.transform.position = _cam.WorldToScreenPoint(_troop.transform.position);
            var toopSelected = false;
            hand.onClick.AddListener(() =>
            {
               // var targetTile = Board.Instance.GetTileFromMousePoint(true);
              //  if (targetTile.GetTroop() != null)
                { 
                    toopSelected = true;
                  //  targetTile.GetTroop().SelectMe();
                }
            });
            yield return new WaitUntil(() => toopSelected && Input.GetMouseButtonUp(0));
            
            hand.onClick.RemoveAllListeners();
            hand.gameObject.SetActive(false);
            hintsPanels[1].SetActive(true);
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            
            var troopMoved = false;
            hintsPanels[1].SetActive(false);
            hand.gameObject.SetActive(true);
            hand.onClick.RemoveAllListeners();
            hand.onClick.AddListener(() =>
            {
               // Player.Instance.MoveTroopFromTutorial(() => troopMoved = true);
            });
            yield return new WaitUntil(()=>troopMoved);
            
            var turnEnds = false;
            hand.onClick.RemoveAllListeners();
            hintsPanels[2].SetActive(true);
            hand.transform.position = endTurnButton.transform.position;
            hand.onClick.AddListener(()=>
            {
                turnEnds = true;
                endTurnButton.onClick?.Invoke();
            });
            yield return new WaitUntil(() => turnEnds);
            
            _diceSummoned = false;
            mask.enabled = false;
            hand.onClick.RemoveAllListeners();
            hand.gameObject.SetActive(false);
            hintsPanels[2].SetActive(false);
            _secondSummon = true;
            yield return new WaitUntil(() => _diceSummoned);
            
            hintsPanels[3].SetActive(true);
            _diceSummoned = false;
            yield return new WaitUntil(()=> Input.GetMouseButtonDown(0));
            
            hintsPanels[3].SetActive(false);
            mask.enabled = true;
            _panelClosed?.Invoke();
            _panelClosed = null;
            //yield return new WaitUntil(() => Player.Instance.GetTroopsCount() >= 2);
            
            toopSelected = false;
           // var myTroops = Player.Instance.GetTroopsOnBoard().ToList();
          //  var troop = myTroops.OrderByDescending(t => t.transform.position.z).FirstOrDefault();
          //  hand.transform.position = _cam.WorldToScreenPoint(troop.transform.position);
            hand.gameObject.SetActive(true);
            hand.onClick.RemoveAllListeners();
            hand.onClick.AddListener(() =>
            {
               // var targetTile = Board.Instance.GetTileFromMousePoint(true);
               // if (targetTile.GetTroop() != null)
                { 
                    toopSelected = true;
               //     targetTile.GetTroop().SelectMe();
                }
            });
            yield return new WaitUntil(() => toopSelected);
            
            hand.gameObject.SetActive(false);
            hintsPanels[4].SetActive(true);
            yield return new WaitUntil(()=> Input.GetMouseButtonUp(0));
            yield return new WaitUntil(()=> Input.GetMouseButtonDown(0));
            
            hintsPanels[4].SetActive(false);
            hand.gameObject.SetActive(true);
            //yield return new WaitUntil(() => _troopAttacked && Player.Instance.AllTroopsAvailable());
            
            hand.onClick.RemoveAllListeners();
            hintsPanels[2].SetActive(true);
            turnEnds = false;
            hand.transform.position = endTurnButton.transform.position;
            hand.onClick.AddListener(()=>
            {
                turnEnds = true;
                endTurnButton.onClick?.Invoke();
            });
            yield return new WaitUntil(()=> turnEnds);
            
            hand.gameObject.SetActive(false);
            hintsPanels[2].SetActive(false);
            yield return new WaitUntil(()=> TurnsManager.Instance.IsPlayerTurn(PlayerTeam.Player1) && TurnsManager.TurnStats == TurnStats.TroopPlay);
            
            toopSelected = false;
          //  myTroops = Player.Instance.GetTroopsOnBoard().ToList();
          //  troop = myTroops.OrderByDescending(t => t.transform.position.z).FirstOrDefault();
          //  hand.transform.position = _cam.WorldToScreenPoint(troop.transform.position);
            hand.gameObject.SetActive(true);
            hand.onClick.RemoveAllListeners();
            hand.onClick.AddListener(() =>
            {
               // var targetTile = Board.Instance.GetTileFromMousePoint(true);
               // if (targetTile.GetTroop() != null)
                { 
                    toopSelected = true;
                 //   targetTile.GetTroop().SelectMe();
                }
            });
            _troopAttacked = false;
            yield return new WaitUntil(() => _troopAttacked);
            
            hand.gameObject.SetActive(false);
            mask.enabled = false;
        }
        

       // public AbilityType[] GetDiceResults()
       // {
         //   _diceIndex++;
         //   return diceResults[_diceIndex-1].diceResults;
       // }
    }
}
