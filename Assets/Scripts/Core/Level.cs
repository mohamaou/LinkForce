using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/Level", order = 1)]
    public class Level : ScriptableObject
    {
        public int summonTime;
        public int coinsToAddOnTurn;
        public int coinsToAddOnMerge;
        public int coinsPerSpawn;
        public int coinsPerRefund;
    }
}