using UnityEngine;
using TMPro;

public class WalletUIController : MonoBehaviour
{
    public static WalletUIController Instance { get; private set; }

    [Header("Главные элементы")]
    public TextMeshProUGUI totalMoneyText;
    public GameObject expandedRibbon;

    [Header("Тексты копилок")]
    public TextMeshProUGUI mandatoryText;
    public TextMeshProUGUI optionalText;
    public TextMeshProUGUI dreamText;

    [Header("Окно с прокруткой")]
    public RectTransform scrollWindowPopup; 
    public TextMeshProUGUI popupTitleText; 

    [Header("Ссылки на сами кнопки копилок")]
    public RectTransform mandatoryButton;
    public RectTransform optionalButton;
    public RectTransform dreamButton;

    [Header("Настройки смещения окна")]
    [Tooltip("Смещение окна вверх относительно центра свинки")]
    public float yOffset = 250f; 

    private bool isExpanded = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // На всякий случай гарантированно выключаем окно при старте
        if (scrollWindowPopup != null) scrollWindowPopup.gameObject.SetActive(false);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (MoneyManager.Instance == null) return;

        totalMoneyText.text = $"{MoneyManager.Instance.TotalMoney} руб.";
        mandatoryText.text = $"Обязательные\n{MoneyManager.Instance.MandatoryMoney} руб.";
        optionalText.text = $"Необязательные\n{MoneyManager.Instance.OptionalMoney} руб.";
        dreamText.text = $"Мечта\n{MoneyManager.Instance.DreamMoney} руб.";
    }

    public void ToggleRibbon()
    {
        isExpanded = !isExpanded;
        expandedRibbon.SetActive(isExpanded);
        
        // Если закрываем ленту — закрываем и попап с деньгами
        if (!isExpanded && scrollWindowPopup != null) 
        {
            scrollWindowPopup.gameObject.SetActive(false);
        }
        
        if (isExpanded) UpdateUI();
    }

    public void OpenPiggyBankDetails(string bankType)
    {
        if (scrollWindowPopup == null) return;

        // Включаем окно
        scrollWindowPopup.gameObject.SetActive(true);

        RectTransform targetButton = null;

        switch (bankType.ToLower())
        {
            case "mandatory": 
                popupTitleText.text = "Обязательные траты"; 
                targetButton = mandatoryButton;
                break;
            case "optional": 
                popupTitleText.text = "Необязательные траты"; 
                targetButton = optionalButton;
                break;
            case "dream": 
                popupTitleText.text = "Моя мечта"; 
                targetButton = dreamButton;
                break;
        }

        if (targetButton != null)
        {
            // Используем transform.position вместо anchoredPosition. 
            // Это заставит UI встать ровно по центру свинки на любом экране!
            scrollWindowPopup.position = targetButton.position;
            
            // А теперь сдвигаем локально вверх с помощью настраиваемого ползунка yOffset
            Vector3 localPos = scrollWindowPopup.localPosition;
            localPos.y += yOffset; 
            scrollWindowPopup.localPosition = localPos;
        }
    }

    public void ClosePopup()
    {
        if (scrollWindowPopup != null) scrollWindowPopup.gameObject.SetActive(false);
    }
}
