using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SupermarketManager : MonoBehaviour
{
    public static SupermarketManager Instance; 

    [Header("Склады продуктов (Заполняем в инспекторе)")]
    public ProductData[] foodProducts;
    public ProductData[] chemicalProducts;
    public ProductData[] entertainmentProducts;

    [Header("Физические Ячейки: ЕДА (3 игровых объекта на полке)")]
    public PhysicsDragAndDrop[] foodItems;
    public TextMeshProUGUI[] foodPriceTexts;

    [Header("Физические Ячейки: ХИМИЯ (3 игровых объекта на полке)")]
    public PhysicsDragAndDrop[] chemItems;
    public TextMeshProUGUI[] chemPriceTexts;

    [Header("Физические Ячейки: РАЗВЛЕЧЕНИЯ (3 игровых объекта на полке)")]
    public PhysicsDragAndDrop[] funItems;
    public TextMeshProUGUI[] funPriceTexts;

    [Header("Касса и монитор")]
    public TextMeshProUGUI monitorTotalText;

    [HideInInspector] public bool isSelectionLocked = false;

    private int foodStartIndex = 0;
    private int chemStartIndex = 0;
    private int funStartIndex = 0;
    
    private List<ProductData> productsOnCounter = new List<ProductData>();

    private int[] foodDiscounts;
    private int[] chemDiscounts;
    private int[] funDiscounts;

    void Awake() 
    { 
        Instance = this; 
    }

    void Start()
    {
        foodDiscounts = new int[foodProducts.Length];
        chemDiscounts = new int[chemicalProducts.Length];
        funDiscounts = new int[entertainmentProducts.Length];

        GenerateGlobalDiscounts();
        
        InitializePositions(foodItems);
        InitializePositions(chemItems);
        InitializePositions(funItems);

        StartCoroutine(DelayedStartUpdate());
    }

    IEnumerator DelayedStartUpdate()
    {
        yield return new WaitForEndOfFrame();
        UpdateAllShelves();
    }

    void GenerateGlobalDiscounts()
    {
        System.Array.Clear(foodDiscounts, 0, foodDiscounts.Length);
        System.Array.Clear(chemDiscounts, 0, chemDiscounts.Length);
        System.Array.Clear(funDiscounts, 0, funDiscounts.Length);

        List<System.Tuple<int, int>> allGlobalProducts = new List<System.Tuple<int, int>>();

        for (int i = 0; i < foodProducts.Length; i++) if (foodProducts[i] != null) allGlobalProducts.Add(new System.Tuple<int, int>(0, i));
        for (int i = 0; i < chemicalProducts.Length; i++) if (chemicalProducts[i] != null) allGlobalProducts.Add(new System.Tuple<int, int>(1, i));
        for (int i = 0; i < entertainmentProducts.Length; i++) if (entertainmentProducts[i] != null) allGlobalProducts.Add(new System.Tuple<int, int>(2, i));

        if (allGlobalProducts.Count == 0) return;

        int totalDiscountsToGive = Random.Range(2, 4);
        totalDiscountsToGive = Mathf.Min(totalDiscountsToGive, allGlobalProducts.Count);

        for (int i = 0; i < allGlobalProducts.Count; i++)
        {
            var temp = allGlobalProducts[i];
            int randomIndex = Random.Range(i, allGlobalProducts.Count);
            allGlobalProducts[i] = allGlobalProducts[randomIndex];
            allGlobalProducts[randomIndex] = temp;
        }

        for (int i = 0; i < totalDiscountsToGive; i++)
        {
            int shelfType = allGlobalProducts[i].Item1;
            int productIndex = allGlobalProducts[i].Item2;
            int discountValue = Random.Range(2, 11) * 5; 

            if (shelfType == 0) foodDiscounts[productIndex] = discountValue;
            else if (shelfType == 1) chemDiscounts[productIndex] = discountValue;
            else if (shelfType == 2) funDiscounts[productIndex] = discountValue;
        }
    }

    void InitializePositions(PhysicsDragAndDrop[] items)
    {
        foreach (var item in items)
        {
            if (item != null)
            {
                item.SetStartPosition(item.transform.position);
            }
        }
    }

    public void NextFood() { if (foodProducts.Length > 0) { foodStartIndex = (foodStartIndex + 1) % foodProducts.Length; UpdateShelf(0); } }
    public void PrevFood() { if (foodProducts.Length > 0) { foodStartIndex = (foodStartIndex - 1 + foodProducts.Length) % foodProducts.Length; UpdateShelf(0); } }

    public void NextChem() { if (chemicalProducts.Length > 0) { chemStartIndex = (chemStartIndex + 1) % chemicalProducts.Length; UpdateShelf(1); } }
    public void PrevChem() { if (chemicalProducts.Length > 0) { chemStartIndex = (chemStartIndex - 1 + chemicalProducts.Length) % chemicalProducts.Length; UpdateShelf(1); } }

    public void NextFun() { if (entertainmentProducts.Length > 0) { funStartIndex = (funStartIndex + 1) % entertainmentProducts.Length; UpdateShelf(2); } }
    public void PrevFun() { if (entertainmentProducts.Length > 0) { funStartIndex = (funStartIndex - 1 + entertainmentProducts.Length) % entertainmentProducts.Length; UpdateShelf(2); } }

    public void UpdateAllShelves()
    {
        UpdateShelf(0);
        UpdateShelf(1);
        UpdateShelf(2);
    }

    void UpdateShelf(int shelfType)
    {
        ProductData[] dataArray = (shelfType == 0) ? foodProducts : (shelfType == 1) ? chemicalProducts : entertainmentProducts;
        PhysicsDragAndDrop[] uiItems = (shelfType == 0) ? foodItems : (shelfType == 1) ? chemItems : funItems;
        TextMeshProUGUI[] priceTexts = (shelfType == 0) ? foodPriceTexts : (shelfType == 1) ? chemPriceTexts : funPriceTexts;
        int startIndex = (shelfType == 0) ? foodStartIndex : (shelfType == 1) ? chemStartIndex : funStartIndex;
        int[] discountArray = (shelfType == 0) ? foodDiscounts : (shelfType == 1) ? chemDiscounts : funDiscounts;
        
        if (dataArray.Length == 0 || uiItems.Length == 0) return;

        for (int i = 0; i < uiItems.Length; i++)
        {
            if (uiItems[i] == null) continue;

            int targetProductIndex = (startIndex + i) % dataArray.Length;
            ProductData p = dataArray[targetProductIndex];

            if (p == null || p.productSprite == null)
            {
                uiItems[i].gameObject.SetActive(false);
                if (priceTexts.Length > i && priceTexts[i] != null) priceTexts[i].text = "";
                continue;
            }

            uiItems[i].gameObject.SetActive(true);
            p.currentDiscount = discountArray[targetProductIndex];
            uiItems[i].currentProduct = p;

            if (productsOnCounter.Contains(p))
            {
                var sr = uiItems[i].GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = null;

                var col = uiItems[i].GetComponent<PolygonCollider2D>();
                if (col != null) col.enabled = false;

                if (priceTexts.Length > i && priceTexts[i] != null) priceTexts[i].text = "";
                continue;
            }

            var spriteRenderer = uiItems[i].GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) 
            {
                spriteRenderer.sprite = p.productSprite;
                spriteRenderer.sortingOrder = 10; 
            }

            uiItems[i].ResetToShelfPosition();

            PolygonCollider2D polyCollider = uiItems[i].GetComponent<PolygonCollider2D>();
            if (polyCollider != null) 
            {
                polyCollider.enabled = false;
                Destroy(polyCollider);        
            }
            uiItems[i].gameObject.AddComponent<PolygonCollider2D>();

            if (priceTexts.Length > i && priceTexts[i] != null)
            {
                if (p.currentDiscount > 0)
                    priceTexts[i].text = $"<s>{p.basePrice}р.</s> <color=red>{p.GetFinalPrice()}р.\n(-{p.currentDiscount}%)</color>";
                else
                    priceTexts[i].text = $"{p.basePrice} р.";
            }
        }
    }

    public void HandleProductDroppedOnCounter(PhysicsDragAndDrop shelfItem)
    {
        if (shelfItem == null || shelfItem.currentProduct == null)
            return;

        ProductData productToBuy = shelfItem.currentProduct;

        if (productsOnCounter.Contains(productToBuy))
            return;

        productsOnCounter.Add(productToBuy);
        UpdateMonitor();

        // Создаём копию товара для стола.
        GameObject counterClone = new GameObject("CounterProduct");

        counterClone.transform.position = shelfItem.transform.position;
        counterClone.transform.rotation = shelfItem.transform.rotation;

        // Сохраняем масштаб товара с полки.
        counterClone.transform.localScale = shelfItem.transform.localScale;

        SpriteRenderer sr = counterClone.AddComponent<SpriteRenderer>();
        sr.sprite = productToBuy.productSprite;
        sr.sortingOrder = 11;

        Rigidbody2D rb = counterClone.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;

        PolygonCollider2D collider = counterClone.AddComponent<PolygonCollider2D>();
        collider.isTrigger = false;

        // Добавляем возможность снова перетаскивать товар со стола.
        PhysicsDragAndDrop drag = counterClone.AddComponent<PhysicsDragAndDrop>();
        drag.currentProduct = productToBuy;
        drag.isOnCounter = true;
        drag.SetStartPosition(counterClone.transform.position);

        // Возвращаем исходный физический объект на полку.
        shelfItem.ResetToShelfPosition();

        UpdateAllShelves();
    }

    public void RemoveCounterProduct(PhysicsDragAndDrop counterItem)
    {
        if (counterItem == null || counterItem.currentProduct == null)
            return;

        ProductData product = counterItem.currentProduct;

        if (productsOnCounter.Contains(product))
        {
            productsOnCounter.Remove(product);
            UpdateMonitor();
            UpdateAllShelves();
        }
    }

    public void RemoveProductFromCart(ProductData product)
    {
        if (product != null && productsOnCounter.Contains(product))
        {
            productsOnCounter.Remove(product);
            UpdateMonitor();
            UpdateAllShelves();
        }
    }

    void UpdateMonitor()
    {
        float currentTotal = 0f;
        foreach (var prod in productsOnCounter)
        {
            currentTotal += prod.GetFinalPrice();
        }

        if (monitorTotalText != null) 
        {
            monitorTotalText.text = $"ИТОГО:\n{currentTotal:F2} р.";
        }
    }
}
