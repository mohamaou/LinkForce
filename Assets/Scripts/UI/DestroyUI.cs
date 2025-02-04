using Core;
using TMPro;
using UnityEngine;

namespace UI
{
    public class DestroyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI refundText;

        private void Start()
        {
            SetRefundText(CoinsManager.Instance.GetCoinsPerRefund());
        }

        public void SetRefundText(int amount)
        {
            refundText.text = "+" + amount;
        }
    }
}