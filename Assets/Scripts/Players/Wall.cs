using UnityEngine;


namespace Players
{
    public class Wall : MonoBehaviour
    {
       
        [SerializeField] private Transform[] wallPoints;
       


        private void Start()
        {
           
        }


       

        public Vector3 GetTargetPoint(Vector3 position)
        {
            var closestDistance = float.MaxValue;
            var closestPoint = wallPoints[0].position;
            for (int i = 0; i < wallPoints.Length; i++)
            {
                var dist = Vector3.Distance(position, wallPoints[i].position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPoint = wallPoints[i].position;
                }
            }
            return closestPoint;
        }

        private void Death()
        {
           
        }
    }
}
