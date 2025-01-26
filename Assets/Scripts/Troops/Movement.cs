using UnityEngine;



namespace Troops
{
    
    public class Movement : MonoBehaviour
    {
        [SerializeField] private Troop troop;
        
        private void Awake()
        {
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