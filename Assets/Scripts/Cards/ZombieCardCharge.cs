using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Cards
{
    public class ZombieCardCharge : MonoBehaviour
    {
        [SerializeField] private Image charge;
        [SerializeField] private Image[] zombieSprites;
        private float _chargeTime;

        public void SetCard(ZombieCard zombie)
        {
            for (int i = 0; i < zombieSprites.Length; i++)
            {
                zombieSprites[i].sprite = zombie.GetSprite();
            }
            _chargeTime = zombie.GetChargeTime();
            charge.fillAmount = 0;
            
        }

        public void StartCharge(System.Action<ZombieCardCharge> spawnZombie)
        {
            charge.DOFillAmount(1, _chargeTime).OnComplete(() => spawnZombie?.Invoke(this));
        }
    }
}
