using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Start_UI
{
    public class CardsGrid : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        private List<Vector3> _gridPositions = new List<Vector3>();
        private bool _start;


        private void Start()
        {
            Invoke(nameof(SetCards),0.2f);
        }

        private void SetCards()
        {
            _start = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                _gridPositions.Add(transform.GetChild(i).localPosition);
            }
            gridLayoutGroup.enabled = false;
        }

        private void Update()
        {
            if (!_start) return;
            for (int i = 0; i < transform.childCount; i++)
            {
                var reverseIndex = _gridPositions.Count - 1 - i;
                transform.GetChild(i).localPosition = _gridPositions[reverseIndex];
            }
        }
    }
}
