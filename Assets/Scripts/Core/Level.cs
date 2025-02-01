using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/Level", order = 1)]
    public class Level : ScriptableObject
    {
        public int summonTime;
        public int coinsPerTurn;
        public int coinsPerTurnWin;
        public int coinsPerTurnLoss;
        public int coinsPerMerge;
        public int coinsPerSpawn;
        public int coinsPerRefund;
    }
}