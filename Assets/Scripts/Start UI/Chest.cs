using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Start_UI
{
    public class Chest : MonoBehaviour
    {
       [SerializeField] private Button chestButton;
       [SerializeField] private GameObject full, empty;
       [SerializeField] private Reward reward;
       private bool _isFull = true;

       private void Awake()
       {
           SetChest();
       }

       private void SetChest()
       {
           _isFull = reward.IsFull();
           full.SetActive(_isFull);
           empty.SetActive(!_isFull);
           if (_isFull) chestButton.onClick.AddListener(OpenChest);
       }

       private void OpenChest()
       {
           if (!_isFull) return;
           StartCoroutine(LootPanel.Instance.CreateBox(reward.GetReward().ToList()));
           reward.full = false;
           reward.SaveData();
           SetChest();
       }
    }
}
