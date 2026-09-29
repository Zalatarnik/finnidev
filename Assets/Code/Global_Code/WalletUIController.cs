using UnityEngine;
using TMPro;

public class WalletUIController : MonoBehaviour
{
    public static WalletUIController Instance { get; private set; }

    [Header("Главные элементы")]
    public GameObject expandedRibbon;
    public TextMeshProUGUI totalMoneyText;

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

    private Canvas myCanvas; 
    private bool isExpanded = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        myCanvas = GetComponent<Canvas>();
    }

    void Start()
    {
        if (scrollWindowPopup != null) scrollWindowPopup.gameObject.SetActive(false);
        UpdateUI();

        if (myCanvas != null)
        {
            myCanvas.enabled = false; 
        }
    }

    public void UpdateUI()
    {
        if (MoneyManager.Instance == null) return;

        totalMoneyText.text = $"{MoneyManager.Instance.TotalMoney} денег";
        mandatoryText.text = $"Обязательные\n{MoneyManager.Instance.MandatoryMoney} денег";
        optionalText.text = $"Необязательные\n{MoneyManager.Instance.OptionalMoney} денег";
        dreamText.text = $"Мечта\n{MoneyManager.Instance.DreamMoney} денег";
    }


    public void ToggleRibbon()
    {
        isExpanded = !isExpanded;
        expandedRibbon.SetActive(isExpanded);
        
        if (!isExpanded && scrollWindowPopup != null) 
        {
            scrollWindowPopup.gameObject.SetActive(false);
        }
        
        if (isExpanded) UpdateUI();
    }

    public void OpenPiggyBankDetails(string bankType)
    {
        if (scrollWindowPopup == null) return;

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
            scrollWindowPopup.position = targetButton.position;
            Vector3 localPos = scrollWindowPopup.localPosition;
            localPos.y += yOffset; 
            scrollWindowPopup.localPosition = localPos;
        }

        PiggyBankContentManager.Instance?.RefreshScrollViewUI(bankType);
    }

    public void ClosePopup()
    {
        if (scrollWindowPopup != null) scrollWindowPopup.gameObject.SetActive(false);
    }
}
