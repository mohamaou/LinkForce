using UnityEngine;



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