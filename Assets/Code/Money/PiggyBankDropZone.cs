using UnityEngine;
using UnityEngine.EventSystems;

public class PiggyBankDropZone : MonoBehaviour, IDropHandler
{
    [Header("Тип копилки (mandatory / optional / dream)")]
    public string bankType;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableMoneyItem moneyItem = eventData.pointerDrag.GetComponent<DraggableMoneyItem>();
            
            if (moneyItem != null && moneyItem.moneyData != null)
            {
                bool isFromTable = TableMoneyManager.Instance != null && 
                                   moneyItem.GetStartParent() == TableMoneyManager.Instance.spawnArea;

                if (!isFromTable && PiggyBankContentManager.Instance != null)
                {
                    string currentOpenBank = PiggyBankContentManager.Instance.GetCurrentActiveBank();
                    
                    if (currentOpenBank == bankType.ToLower()) return;

                    MoneyManager.Instance.ChangeMoney(currentOpenBank, -moneyItem.moneyData.nominal);
                    PiggyBankContentManager.Instance.RemoveMoneyFromBank(currentOpenBank, moneyItem.moneyData);
                }

                MoneyManager.Instance.ChangeMoney(bankType, moneyItem.moneyData.nominal);
                PiggyBankContentManager.Instance.AddMoneyToBank(bankType, moneyItem.moneyData);
                
                if (isFromTable)
                {
                    TableMoneyManager.Instance.MoneyItemCollected();
                }
                
                moneyItem.SuccessfullyDropped();
            }
        }
    }
}
