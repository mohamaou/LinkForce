using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Players;
using Troops;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core
{
    public class Tutorial : MonoBehaviour
    {
        public static Tutorial Instance { get; private set; }
        [SerializeField] private Button hand;
        [SerializeField] private Image mask;
        [SerializeField] private GameObject[] hintsPanels;
        private Camera _cam;
        private bool _buildingLinks;
     
        

        private void Awake()
        {
            Instance = this;
            _cam = Camera.main;
        }

        private IEnumerator Start()
        {
            hand.gameObject.SetActive(false);
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
            yield return new WaitForSeconds(2);

            //Summon First Building
            ShowHintText(0);
            yield return SummonBuilding(0);
            yield return new WaitForSeconds(1);
            //Summon Second Building
            yield return SummonBuilding(1);
            //Link The Tow Building Together
            ShowHintText(1);
            yield return LinkBuilding(TroopType.Human, TroopType.Sword);
            yield return new WaitForSeconds(0.5f);
            //Starr Battle
            ShowHintText(2);
            yield return BattleButtonClicked();
            ShowHintText();

            //Round 2
            yield return new WaitUntil(()=> TurnsManager.PlayState == PlayState.Summon);
            yield return SummonBuilding(2);
            yield return SummonBuilding(0);
            ShowHintText(1);
            yield return LinkBuilding(TroopType.Human, TroopType.Bow);
            ShowHintText();
            yield return BattleButtonClicked();

            //Round 3
            yield return new WaitUntil(()=> TurnsManager.PlayState == PlayState.Summon);
            yield return SummonBuilding(3);
            yield return LinkBuilding(TroopType.Sword, TroopType.Armor);
            yield return MergeBuilding(TroopType.Human, TroopType.Human);
            yield return BattleButtonClicked();
            
            
        }

        private IEnumerator ButtonClicked()
        {
            hand.gameObject.SetActive(true);
            var buttonClicked = false;
            hand.onClick.RemoveAllListeners();
            hand.onClick.AddListener(() => { buttonClicked = true; });
            yield return new WaitUntil(() => buttonClicked);
        }
        private IEnumerator LinkBuilding(TroopType source, TroopType target)
        {
            hand.gameObject.SetActive(true);
            var buildings = Player.Instance.GetBuildingsOnBoard;
            var s = buildings
                .Where(b => b.GetEquipmentType() == source)
                .OrderBy(b => b.GetAllLinks().Count)
                .FirstOrDefault();

            var t = buildings.FirstOrDefault(b => b.GetEquipmentType() == target);
            hand.transform.position = _cam.WorldToScreenPoint(s.transform.position);
            hand.transform.DOKill();
            hand.transform.DOMove(_cam.WorldToScreenPoint(t.transform.position), 1f).SetLoops(-1, LoopType.Restart);
            yield return new WaitUntil(() => _buildingLinks);
            _buildingLinks = false;
            hand.gameObject.SetActive(false);
        }
        private IEnumerator MergeBuilding(TroopType source, TroopType target)
        {
            hand.gameObject.SetActive(true);
            var buildings = Player.Instance.GetBuildingsOnBoard;
            var s = buildings.LastOrDefault(b => b.GetEquipmentType() == source);
            var t = buildings.FirstOrDefault(b => b.GetEquipmentType() == target && b != s);
            hand.transform.position = _cam.WorldToScreenPoint(s.transform.position);
            hand.transform.DOKill();
            hand.transform.DOMove(_cam.WorldToScreenPoint(t.transform.position), 1f).SetLoops(-1, LoopType.Restart);
            yield return new WaitUntil(() => _buildingLinks);
            _buildingLinks = false;
            hand.gameObject.SetActive(false);
        }
        private IEnumerator BattleButtonClicked()
        {
            hand.gameObject.SetActive(true);
            hand.transform.position = UIManager.Instance.playPanel.GetBattleButton().transform.position;
            ScaleHand();
            var buttonClicked = false;
            hand.onClick.RemoveAllListeners();
            hand.onClick.AddListener(() =>
            {
                UIManager.Instance.playPanel.GetBattleButton().onClick.Invoke();
                buttonClicked = true;
            });
            yield return new WaitUntil(() => buttonClicked);
            hand.gameObject.SetActive(false);
        }

        private IEnumerator SummonBuilding(int index)
        {
            hand.gameObject.SetActive(true);
            hand.transform.position = UIManager.Instance.playPanel.GetSummonButton().transform.position;
            ScaleHand();
            yield return ButtonClicked();
            Player.Instance.SummonBuildingEditor(index);
            hand.gameObject.SetActive(false);
        }

        private void ScaleHand()
        {
            hand.transform.DOKill();
            hand.transform.DOScale(Vector3.one  * 1.2f,.5f).SetLoops(-1, LoopType.Yoyo);
        }
        public void BuildingLinked()
        {
            _buildingLinks = true;
        }

        private void ShowHintText(int index = 100)
        {
            for (int i = 0; i < hintsPanels.Length; i++)
            {
                hintsPanels[i].SetActive(index == i);
            }
        }
    }
}