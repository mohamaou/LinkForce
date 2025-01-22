using Cards;
using Core;
using DG.Tweening;
using JetBrains.Annotations;
using Level;
using Players;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UpgradePanel : MonoBehaviour
    {
       [SerializeField] private Button mainUpgradeButton, upgradeButton1, upgradeButton2;
       [SerializeField] private Image upgrade1Image, upgrade2Image, age1Type, age2Type;
       [SerializeField] private TextMeshProUGUI upgrade1CostText, upgrade2CostText;
       [SerializeField] private Sprite economy, military;
       private Vector3 _upgrade1Position, _upgrade2Position;
       private bool _opened;
    

       private void Start()
       {
           _upgrade1Position = upgradeButton1.transform.position;
           _upgrade2Position = upgradeButton2.transform.position;
           
           upgradeButton1.gameObject.SetActive(false);
           upgradeButton2.gameObject.SetActive(false);
           mainUpgradeButton.onClick.AddListener(UpgradeButtonClicked);
           upgradeButton1.onClick.AddListener(() =>
           {
             /*  var cost = AgeManager.Instance.GetNextAgeUpgradeCost(Player.Instance.GetCurrentAge());
               if (InGameCurrency.Instance.IsXPEnough(cost))
               {
                   InGameCurrency.Instance.AddXp(-cost);
                   Close(); 
                   Player.Instance.LevelUp(AgeManager.Instance.GetNexAge(Player.Instance.GetCurrentAge(),0)); 
                   SetButtonsAges();
               }*/
           });
           upgradeButton2.onClick.AddListener(() =>
           {
              /* var cost = AgeManager.Instance.GetNextAgeUpgradeCost(Player.Instance.GetCurrentAge());
               if (InGameCurrency.Instance.IsXPEnough(cost))
               {
                   InGameCurrency.Instance.AddXp(-cost);
                   Close(); 
                   Player.Instance.LevelUp(AgeManager.Instance.GetNexAge(Player.Instance.GetCurrentAge(),1)); 
                   SetButtonsAges()/
               }*/
           });
       }
       
       public void UpgradeButtonClicked()
       {
           _opened = !_opened;
           if (_opened) Open();
           else Close();
           SetButtonsAges();
       }
       private void SetButtonsAges()
       {
         /*  var ageData = AgeManager.Instance.GetNexAgeData(Player.Instance.GetCurrentAge());
           if (ageData == null)
           {
               mainUpgradeButton.gameObject.SetActive(false);
               upgradeButton1.gameObject.SetActive(false);
               upgradeButton2.gameObject.SetActive(false);
               return;
           }
           if (ageData.Length > 1)
           {
               upgrade1Image.sprite = ageData[0].GetAgeSprite();
               upgrade2Image.sprite = ageData[1].GetAgeSprite();
               age1Type.sprite = ageData[0].GetAgeType()== AgeType.Economy ? economy : military;
               age2Type.sprite = ageData[1].GetAgeType()== AgeType.Economy ? economy : military;
           }
           else
           {
               upgrade1Image.sprite = ageData[0].GetAgeSprite();
               age1Type.sprite = ageData[0].GetAgeType()== AgeType.Economy ? economy : military;
               upgradeButton2.gameObject.SetActive(false);
           }
           upgrade1CostText.text = $"{AgeManager.Instance.GetAgeUpgradeCost(ageData[0].GetAge())} XP";
           upgrade2CostText.text = $"{AgeManager.Instance.GetAgeUpgradeCost(ageData[0].GetAge())} XP";
           var isXpEnough = InGameCurrency.Instance.IsXPEnough(AgeManager.Instance.GetAgeUpgradeCost(ageData[0].GetAge()));
           upgrade1Image.color = upgrade2Image.color = isXpEnough ? Color.white: Color.gray;
           upgrade1CostText.color = upgrade2CostText.color = isXpEnough ? Color.green : Color.red;*/
       }
       private void Open()
       {
           _opened = true;
           upgradeButton1.transform.position = mainUpgradeButton.transform.position;
           upgradeButton2.transform.position = mainUpgradeButton.transform.position;
           upgradeButton1.gameObject.SetActive(true);
           upgradeButton2.gameObject.SetActive(true);
           upgradeButton2.transform.localScale = upgradeButton2.transform.localScale = Vector3.zero;
           upgradeButton1.transform.DOMove(_upgrade1Position, 0.2f);
           upgradeButton1.transform.DOScale(Vector3.one, 0.2f);
           upgradeButton2.transform.DOMove(_upgrade2Position, 0.2f);
           upgradeButton2.transform.DOScale(Vector3.one, 0.2f);
       }
       private void Close()
       {
           _opened = false;
           upgradeButton2.transform.localScale = upgradeButton2.transform.localScale  = Vector3.zero;
           upgradeButton1.transform.DOMove(mainUpgradeButton.transform.position, 0.2f);
           upgradeButton1.transform.DOScale(Vector3.one, 0.2f).OnComplete(()=>upgradeButton1.gameObject.SetActive(false));
           upgradeButton2.transform.DOMove(mainUpgradeButton.transform.position, 0.2f);
           upgradeButton2.transform.DOScale(Vector3.one, 0.2f).OnComplete(()=>upgradeButton2.gameObject.SetActive(false));
       }

       private void Update()
       {
          /* if (Input.GetKeyDown(KeyCode.Q))
           {
               Player.Instance.LevelUp(AgeManager.Instance.GetNexAge(Player.Instance.GetCurrentAge(),0)); 
               SetButtonsAges();
           }

           if (Input.GetKeyDown(KeyCode.E))
           {
               Player.Instance.LevelUp(AgeManager.Instance.GetNexAge(Player.Instance.GetCurrentAge(),1)); 
               SetButtonsAges();
           }*/
       }
    }
}
