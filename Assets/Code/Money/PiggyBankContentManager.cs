using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PiggyBankContentManager : MonoBehaviour
{
    public static PiggyBankContentManager Instance { get; private set; }

    [Header("UI элементы")]
    public GameObject moneyDisplayPrefab; // Префаб с компонентом Image для ячейки
    public Transform scrollViewContentContainer; // Сюда перетаскиваем объект Content

    [Header("Все номиналы в игре")]
    public List<MoneyData> allMoneyVariants; // Сюда закинуты Data_100, Data_500 и т.д.

    // Хранилище: <ИмяСвинки, <Номинал_Денег, Количество_Штук>>
    private Dictionary<string, Dictionary<int, int>> bankContents = new Dictionary<string, Dictionary<int, int>>();

    // Запоминаем, какая свинка открыта в данный момент
    private string currentActiveBank = "mandatory";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Создаем чистые независимые кошельки для каждой свинки при старте игры
        bankContents.Add("mandatory", new Dictionary<int, int>());
        bankContents.Add("optional", new Dictionary<int, int>());
        bankContents.Add("dream", new Dictionary<int, int>());
    }

    // Позволяет другим скриптам узнать, какая свинка сейчас открыта на экране
    public string GetCurrentActiveBank()
    {
        return currentActiveBank;
    }

    // Вызывается из скрипта PiggyBankDropZone, когда монетка падает в свинку
    public void AddMoneyToBank(string bankType, MoneyData data)
    {
        string bankKey = bankType.ToLower();
        
        if (!bankContents.ContainsKey(bankKey)) return;

        var targetBank = bankContents[bankKey];
        
        // Если такого номинала еще не было в этой свинке, добавляем его со значением 0
        if (!targetBank.ContainsKey(data.nominal))
        {
            targetBank[data.nominal] = 0;
        }
        
        // Увеличиваем количество штук именно этого номинала именно в этой свинке
        targetBank[data.nominal]++;
        
        // Перерисовываем экран, если мы закинули деньги в ту свинку, чье окно сейчас открыто
        if (currentActiveBank == bankKey)
        {
            RefreshScrollViewUI(bankKey);
        }
    }

    // Вызывается из скрипта PiggyBankDropZone, когда купюру забирают из текущей свинки
    public void RemoveMoneyFromBank(string bankType, MoneyData data)
    {
        string bankKey = bankType.ToLower();
        if (!bankContents.ContainsKey(bankKey)) return;

        var targetBank = bankContents[bankKey];
        if (targetBank.ContainsKey(data.nominal))
        {
            targetBank[data.nominal]--; // Уменьшаем количество штук на 1
            
            // Застрахуем себя от отрицательных значений
            if (targetBank[data.nominal] < 0) targetBank[data.nominal] = 0;
        }
        
        // Сразу перерисовываем вертикальный список, чтобы пачка уменьшилась или исчезла
        RefreshScrollViewUI(bankKey);
    }

    // Вызывается при нажатии на свинку для переключения содержимого окна
    public void RefreshScrollViewUI(string bankType)
    {
        string bankKey = bankType.ToLower();
        currentActiveBank = bankKey; // Фиксируем, какую свинку мы сейчас смотрим

        if (scrollViewContentContainer == null) return;

        // 1. Полностью очищаем старые картинки купюр из окошка
        foreach (Transform child in scrollViewContentContainer)
        {
            Destroy(child.gameObject);
        }

        if (!bankContents.ContainsKey(bankKey)) return;
        var targetBank = bankContents[bankKey];

        // 2. Проверяем каждый номинал по очереди (отдельные ячейки!)
        foreach (MoneyData money in allMoneyVariants)
        {
            // Проверяем, сколько штук этой купюры лежит в ВЫБРАННОЙ свинке
            int count = targetBank.ContainsKey(money.nominal) ? targetBank[money.nominal] : 0;

            // Если в этой свинке есть хотя бы 1 такая купюра/монета
            if (count > 0)
            {
                // Создаем отдельную новую ячейку (префаб) в контейнере Content
                GameObject newDisplay = Instantiate(moneyDisplayPrefab, scrollViewContentContainer);
                
                // Передаем номинал купюры в скрипт перетаскивания, чтобы ячейка знала свою цену
                DraggableMoneyItem dragScript = newDisplay.GetComponent<DraggableMoneyItem>();
                if (dragScript != null)
                {
                    dragScript.moneyData = money;
                }

                Image img = newDisplay.GetComponent<Image>();
                if (img != null)
                {
                    // Запрашиваем у MoneyData правильную картинку (1, 2, пачка или куча) на основе штук
                    img.sprite = money.GetSpriteForCount(count);
                    img.SetNativeSize(); // Сохраняем пропорции картинки из ассета
                }
            }
        }
    }
}
