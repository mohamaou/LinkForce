using UnityEngine;
using Zombies;

namespace Cards
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "ScriptableObjects/Zombie Card", order = 1)]
    public class ZombieCard : ScriptableObject
    {
        [SerializeField] private ZombieType zombie;
        [SerializeField] private Sprite zombieSprite;
        [SerializeField] private float chargeTime = 1;
        [SerializeField] private int cost, unlockLevel;
        
        public Sprite GetSprite() => zombieSprite;
        public ZombieType GetZombie() => zombie;
        public int GetCost() => cost;
        public int GetUnlockLevel() => unlockLevel;
        public float GetChargeTime()=> chargeTime;
    }
}
