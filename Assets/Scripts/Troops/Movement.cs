using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Level;
using UnityEngine;
using Zombies;


namespace Troops
{
    
    public class Movement : MonoBehaviour
    {
        [SerializeField] private Troop troop;
        
        private void Awake()
        {
            troop.SetEvent(() =>
            {
               troop.GetAnimation().Move(false);
            });
            troop.SetDeathEvent(() =>
            {
                Destroy(this);
            });
        }

        
        public void Move()
        {
            
        }
        
    }
    
}