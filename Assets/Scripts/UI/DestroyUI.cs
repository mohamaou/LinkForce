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
            SetRefundText();
        }

        private void SetRefundText()
        {
            refundText.text = "+" + CoinsManager.Instance.GetCoinsPerRefund();
        }
    }
}