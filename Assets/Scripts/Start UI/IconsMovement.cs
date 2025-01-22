using UnityEngine;

namespace UI
{
    public class IconsMovement : MonoBehaviour
    {
        [SerializeField] private RectTransform border;
        [SerializeField] private Transform[] icons;
        [SerializeField] [Range(0,1)]private float speed = 6f;
        private float range;


        private void Awake()
        {
            range = icons[5].localPosition.y - icons[6].localPosition.y;
        }

        private void Update()
        {
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].localPosition += Vector3.up * (speed * Time.deltaTime * Screen.height/10f);
                if (icons[i].localPosition.y > border.rect.height/2) icons[i].localPosition = LowestPoint() - Vector3.up * range;
            }
        }

        private Vector3 LowestPoint()
        {
            var lowest = Vector3.zero;
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i].localPosition.y < lowest.y) lowest = icons[i].localPosition;
            }

            return lowest;
        }
    }
}
