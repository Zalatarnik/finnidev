using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TableMoneyManager : MonoBehaviour
{
    public static TableMoneyManager Instance { get; private set; }

    [Header("Настройки спавна")]
    public Transform spawnArea;        
    public GameObject baseMoneyPrefab; 
    public List<MoneyData> availableMoneyData; 

    [Header("UI элементы")]
    public Button doneButton;          

    private int totalItemsOnTable = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (doneButton != null)
        {
            doneButton.gameObject.SetActive(false);
            doneButton.onClick.AddListener(FinishDistribution);
        }

        if (WalletUIController.Instance != null)
        {
            Canvas walletCanvas = WalletUIController.Instance.GetComponent<Canvas>();
            if (walletCanvas != null) walletCanvas.enabled = true;

            WalletUIController.Instance.expandedRibbon.SetActive(true);
            WalletUIController.Instance.UpdateUI();
        }

        GenerateDailyAllowance();
    }

    void GenerateDailyAllowance()
    {
        if (baseMoneyPrefab == null || spawnArea == null || availableMoneyData == null || availableMoneyData.Count == 0) return;

        int itemsToSpawn = Random.Range(4, 9);

        for (int i = 0; i < itemsToSpawn; i++)
        {
            MoneyData randomData = availableMoneyData[Random.Range(0, availableMoneyData.Count)];
            GameObject spawnedMoney = Instantiate(baseMoneyPrefab, spawnArea);
            
            Image img = spawnedMoney.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = randomData.spriteSingle; 
                img.SetNativeSize(); 
            }

            DraggableMoneyItem dragScript = spawnedMoney.GetComponent<DraggableMoneyItem>();
            if (dragScript != null) dragScript.moneyData = randomData;

            RectTransform rect = spawnedMoney.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(Random.Range(-300f, 300f), Random.Range(-400f, 400f));
                rect.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
            }

            totalItemsOnTable++;
        }
    }

    public void MoneyItemCollected()
    {
        totalItemsOnTable--;
        if (totalItemsOnTable <= 0 && doneButton != null)
        {
            doneButton.gameObject.SetActive(true);
        }
    }

    void FinishDistribution()
    {
        if (WalletUIController.Instance != null)
        {
            WalletUIController.Instance.expandedRibbon.SetActive(false);
            if (WalletUIController.Instance.scrollWindowPopup != null)
            {
                WalletUIController.Instance.scrollWindowPopup.gameObject.SetActive(false);
            }

            Canvas walletCanvas = WalletUIController.Instance.GetComponent<Canvas>();
            if (walletCanvas != null) walletCanvas.enabled = true;

            WalletUIController.Instance.UpdateUI();
        }

        SceneManager.LoadScene("SampleScene"); 
    }
}
