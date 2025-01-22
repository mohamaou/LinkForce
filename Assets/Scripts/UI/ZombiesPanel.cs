using System.Collections.Generic;
using Cards;
using DG.Tweening;
using Level;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;
using Zombies;

namespace UI
{
    public class ZombiesPanel : MonoBehaviour
    { 
        public static ZombiesPanel Instance {get; private set;}
        [SerializeField] private ZombieCardUI[] zombiesCardUi;
        [SerializeField] private ZombieCard[] zombies;
        [SerializeField] private ZombieCardCharge chargeCard;
        [SerializeField] private Image waitLineImage;
        [SerializeField] Transform waitLineHolder, chargeCardsHolder;
        [SerializeField] private MMF_Player zombieClickedFeedback;
        private Color _waitLineColor;
        private readonly List<ZombieCardCharge> _waitLineChildren = new List<ZombieCardCharge>();
        
        
        
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LevelManager.Instance.SetRoundStartsEvents(RoundStart);
            _waitLineColor = waitLineImage.color;
            waitLineImage.color = Color.clear;
        }

        private void RoundStart(int roundCount)
        {
            var availableZombies = new List<ZombieCard>();
            for (int i = 0; i < zombies.Length; i++)
            {
                if(zombies[i].GetUnlockLevel() <= roundCount) availableZombies.Add(zombies[i]);
            }
            availableZombies.Sort((a, b) => b.GetUnlockLevel().CompareTo(a.GetUnlockLevel()));
            if (availableZombies.Count > zombiesCardUi.Length)
            {
                availableZombies.RemoveRange(zombiesCardUi.Length, availableZombies.Count - zombiesCardUi.Length);
            }
            availableZombies.Reverse();
            for (int i = 0; i < zombiesCardUi.Length; i++)
            {
                if (availableZombies.Count > i)
                { 
                    zombiesCardUi[i].SetClockState(false); 
                    zombiesCardUi[i].SetCard(availableZombies[i]);
                }
                else
                {
                    zombiesCardUi[i].SetClockState(true);
                }
            }
        }

        public ZombieCard[] GetAvailableZombies()
        {
            var availableZombies = new List<ZombieCard>();
            for (int i = 0; i < zombiesCardUi.Length; i++)
            {
                if (!zombiesCardUi[i].IsLocked())
                {
                    for (int j = 0; j < zombies.Length; j++)
                    {
                        if(zombies[j].GetZombie() == zombiesCardUi[i].GetZombie())
                            availableZombies.Add(zombies[j]);
                    }
                }
            }
            return availableZombies.ToArray();
        }
        public void SpawnZombie(ZombieType zombie)
        { 
            if(waitLineHolder.childCount <=  _waitLineChildren.Count) return;
            zombieClickedFeedback?.PlayFeedbacks();
            var card = Instantiate(chargeCard, chargeCardsHolder);
            card.transform.localScale = Vector3.zero;
            card.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            card.SetCard(GetZombieCard(zombie));
            waitLineImage.color = _waitLineColor;
            card.transform.position = waitLineHolder.GetChild(_waitLineChildren.Count).position;
            if (_waitLineChildren.Count == 0) card.StartCharge(CardCharged);
            _waitLineChildren.Add(card);
        }

        private void CardCharged(ZombieCardCharge card)
        {
            card.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutBack).OnComplete(() => Destroy(card.gameObject));
            ZombiesSpawner.Instance.SpawnZombie(PlayerTeam.Player2, card.GetZombieType(),false);
            _waitLineChildren.RemoveAt(0);
            if (_waitLineChildren.Count == 0)
            {
                waitLineImage.color = Color.clear;
                return;
            }
            for (int i = 0; i < _waitLineChildren.Count; i++)
            {
                if (i == 0) _waitLineChildren[i].StartCharge(CardCharged);
                _waitLineChildren[i].transform.DOMove(waitLineHolder.GetChild(i).position, 0.2f).SetEase(Ease.OutBack);
            }
        }
        private ZombieCard GetZombieCard(ZombieType zombie)
        {
            for (int i = 0; i < zombies.Length; i++)
            {
                if (zombies[i].GetZombie() == zombie) return zombies[i];
            }
            return null;
        }
    }
}
