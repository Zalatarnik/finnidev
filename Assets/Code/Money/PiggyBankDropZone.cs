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
                // ИСПРАВЛЕНИЕ: Проверяем, откуда ИЗНАЧАЛЬНО была взята купюра перед перетаскиванием
                // Если её стартовый родитель — это зона спавна на столе мамы, значит она ТОЧНО со стола!
                bool isFromTable = TableMoneyManager.Instance != null && 
                                   moneyItem.GetStartParent() == TableMoneyManager.Instance.spawnArea;

                // Если мы перетаскиваем деньги из ОКНА СКРОЛЛА (внутри кошелька)
                if (!isFromTable && PiggyBankContentManager.Instance != null)
                {
                    string currentOpenBank = PiggyBankContentManager.Instance.GetCurrentActiveBank();
                    
                    // Если игрок пытается бросить купюру в ту же самую свинку, откуда взял — ничего не делаем
                    if (currentOpenBank == bankType.ToLower()) return;

                    // Списываем из старой свинки
                    MoneyManager.Instance.ChangeMoney(currentOpenBank, -moneyItem.moneyData.nominal);
                    PiggyBankContentManager.Instance.RemoveMoneyFromBank(currentOpenBank, moneyItem.moneyData);
                }

                // Зачисляем номинал в НОВУЮ выбранную свинку
                MoneyManager.Instance.ChangeMoney(bankType, moneyItem.moneyData.nominal);
                PiggyBankContentManager.Instance.AddMoneyToBank(bankType, moneyItem.moneyData);
                
                // Уменьшаем счетчик стола ТОЛЬКО если денежка была взята с самого стола
                if (isFromTable)
                {
                    TableMoneyManager.Instance.MoneyItemCollected();
                }
                
                // Уничтожаем временный объект перетаскивания
                moneyItem.SuccessfullyDropped();
            }
        }
    }
}
