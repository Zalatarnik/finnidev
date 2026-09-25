using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class SupermarketManager : MonoBehaviour
{
    public static SupermarketManager Instance; 

    [Header("Склады продуктов (Заполняем в инспекторе)")]
    public ProductData[] foodProducts;
    public ProductData[] chemicalProducts;
    public ProductData[] entertainmentProducts;

    [Header("UI Ячейки: ЕДА (3 холдера и 3 текста цен)")]
    public DragAndDropProduct[] foodItems;
    public TextMeshProUGUI[] foodPriceTexts;

    [Header("UI Ячейки: ХИМИЯ (3 холдера и 3 текста цен)")]
    public DragAndDropProduct[] chemItems;
    public TextMeshProUGUI[] chemPriceTexts;

    [Header("UI Ячейки: РАЗВЛЕЧЕНИЯ (3 холдера и 3 текста цен)")]
    public DragAndDropProduct[] funItems;
    public TextMeshProUGUI[] funPriceTexts;

    [Header("Касса и монитор")]
    public TextMeshProUGUI monitorTotalText;

    private int foodStartIndex = 0;
    private int chemStartIndex = 0;
    private int funStartIndex = 0;
    private float totalCartSum = 0f;

    void Awake() { Instance = this; }

    void Start()
    {
        // Генерируем случайные скидки дня
        GenerateRandomDiscounts(foodProducts);
        GenerateRandomDiscounts(chemicalProducts);
        GenerateRandomDiscounts(entertainmentProducts);
        
        // Отрисовываем 3 полки по 3 товара
        UpdateShelf(0); 
        UpdateShelf(1); 
        UpdateShelf(2);
        UpdateMonitor();
    }

    void GenerateRandomDiscounts(ProductData[] products)
    {
        foreach (var prod in products)
        {
            if (prod != null)
            {
                prod.currentDiscount = (Random.value < 0.4f) ? Random.Range(2, 11) * 5 : 0; // Скидки от 10% до 50%
            }
        }
    }

    // Методы для боковых стрелочек
    public void NextFood() { if (foodProducts.Length > 0) { foodStartIndex = (foodStartIndex + 1) % foodProducts.Length; UpdateShelf(0); } }
    public void PrevFood() { if (foodProducts.Length > 0) { foodStartIndex = (foodStartIndex - 1 + foodProducts.Length) % foodProducts.Length; UpdateShelf(0); } }

    public void NextChem() { if (chemicalProducts.Length > 0) { chemStartIndex = (chemStartIndex + 1) % chemicalProducts.Length; UpdateShelf(1); } }
    public void PrevChem() { if (chemicalProducts.Length > 0) { chemStartIndex = (chemStartIndex - 1 + chemicalProducts.Length) % chemicalProducts.Length; UpdateShelf(1); } }

    public void NextFun() { if (entertainmentProducts.Length > 0) { funStartIndex = (funStartIndex + 1) % entertainmentProducts.Length; UpdateShelf(2); } }
    public void PrevFun() { if (entertainmentProducts.Length > 0) { funStartIndex = (funStartIndex - 1 + entertainmentProducts.Length) % entertainmentProducts.Length; UpdateShelf(2); } }

    void UpdateShelf(int shelfType)
    {
        ProductData[] dataArray = (shelfType == 0) ? foodProducts : (shelfType == 1) ? chemicalProducts : entertainmentProducts;
        DragAndDropProduct[] uiItems = (shelfType == 0) ? foodItems : (shelfType == 1) ? chemItems : funItems;
        TextMeshProUGUI[] priceTexts = (shelfType == 0) ? foodPriceTexts : (shelfType == 1) ? chemPriceTexts : funPriceTexts;
        int startIndex = (shelfType == 0) ? foodStartIndex : (shelfType == 1) ? chemStartIndex : funStartIndex;
        
        if (dataArray.Length == 0 || uiItems.Length == 0) return;

        for (int i = 0; i < uiItems.Length; i++)
        {
            if (uiItems[i] == null) continue;

            int productIndex = (startIndex + i) % dataArray.Length;
            ProductData p = dataArray[productIndex];

            uiItems[i].ResetToShelf();
            uiItems[i].GetComponent<Image>().sprite = p.productSprite;
            uiItems[i].currentProduct = p;

            if (priceTexts.Length > i && priceTexts[i] != null)
            {
                if (p.currentDiscount > 0)
                {
                    priceTexts[i].text = $"<s>{p.basePrice}р.</s> <color=red>{p.GetFinalPrice()}р.\n(-{p.currentDiscount}%)</color>";
                }
                else
                {
                    priceTexts[i].text = $"{p.basePrice} р.";
                }
            }
        }
    }

    public void AddProductToCart(ProductData product)
    {
        if (product != null)
        {
            totalCartSum += product.GetFinalPrice();
            UpdateMonitor();
        }
    }

    void UpdateMonitor()
    {
        if (monitorTotalText != null) 
        {
            monitorTotalText.text = $"ИТОГО:\n{totalCartSum:F2} р.";
        }
    }
}
