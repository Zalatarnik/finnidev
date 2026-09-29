using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PiggyBankContentManager : MonoBehaviour
{
    public static PiggyBankContentManager Instance { get; private set; }

    [Header("UI элементы")]
    public GameObject moneyDisplayPrefab;
    public Transform scrollViewContentContainer;

    [Header("Все номиналы в игре")]
    public List<MoneyData> allMoneyVariants;

    private Dictionary<string, Dictionary<int, int>> bankContents = new Dictionary<string, Dictionary<int, int>>();

    private string currentActiveBank = "mandatory";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        bankContents.Add("mandatory", new Dictionary<int, int>());
        bankContents.Add("optional", new Dictionary<int, int>());
        bankContents.Add("dream", new Dictionary<int, int>());
    }

    public string GetCurrentActiveBank()
    {
        return currentActiveBank;
    }

    public void AddMoneyToBank(string bankType, MoneyData data)
    {
        string bankKey = bankType.ToLower();
        
        if (!bankContents.ContainsKey(bankKey)) return;

        var targetBank = bankContents[bankKey];
        
        if (!targetBank.ContainsKey(data.nominal))
        {
            targetBank[data.nominal] = 0;
        }
        
        targetBank[data.nominal]++;
        
        if (currentActiveBank == bankKey)
        {
            RefreshScrollViewUI(bankKey);
        }
    }

    public void RemoveMoneyFromBank(string bankType, MoneyData data)
    {
        string bankKey = bankType.ToLower();
        if (!bankContents.ContainsKey(bankKey)) return;

        var targetBank = bankContents[bankKey];
        if (targetBank.ContainsKey(data.nominal))
        {
            targetBank[data.nominal]--;

            if (targetBank[data.nominal] < 0) targetBank[data.nominal] = 0;
        }
        
        RefreshScrollViewUI(bankKey);
    }

    public void RefreshScrollViewUI(string bankType)
    {
        string bankKey = bankType.ToLower();
        currentActiveBank = bankKey;

        if (scrollViewContentContainer == null) return;

        foreach (Transform child in scrollViewContentContainer)
        {
            Destroy(child.gameObject);
        }

        if (!bankContents.ContainsKey(bankKey)) return;
        var targetBank = bankContents[bankKey];

        foreach (MoneyData money in allMoneyVariants)
        {
            int count = targetBank.ContainsKey(money.nominal) ? targetBank[money.nominal] : 0;

            if (count > 0)
            {
                GameObject newDisplay = Instantiate(moneyDisplayPrefab, scrollViewContentContainer);
                
                DraggableMoneyItem dragScript = newDisplay.GetComponent<DraggableMoneyItem>();
                if (dragScript != null)
                {
                    dragScript.moneyData = money;
                }

                Image img = newDisplay.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = money.GetSpriteForCount(count);
                    img.SetNativeSize();
                }
            }
        }
    }
}
