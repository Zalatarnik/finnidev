using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("Баланс копилок")]
    [SerializeField] private int mandatoryExpensesMoney = 1000;
    [SerializeField] private int optionalExpensesMoney = 500;
    [SerializeField] private int dreamMoney = 5000;

    public int MandatoryMoney => mandatoryExpensesMoney;
    public int OptionalMoney => optionalExpensesMoney;
    public int DreamMoney => dreamMoney;
    public int TotalMoney => mandatoryExpensesMoney + optionalExpensesMoney + dreamMoney;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Так как скрипт находится на объекте Managers внутри [GLOBAL_SYSTEMS],
        // мы делаем бессмертным САМЫЙ КОРНЕВОЙ объект всей системы.
        DontDestroyOnLoad(transform.root.gameObject); 
    }

    public void ChangeMoney(string bankType, int amount)
    {
        switch (bankType.ToLower())
        {
            case "mandatory": mandatoryExpensesMoney = Mathf.Max(0, mandatoryExpensesMoney + amount); break;
            case "optional": optionalExpensesMoney = Mathf.Max(0, optionalExpensesMoney + amount); break;
            case "dream": dreamMoney = Mathf.Max(0, dreamMoney + amount); break;
        }
        WalletUIController.Instance?.UpdateUI();
    }
}
