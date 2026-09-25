using UnityEngine; 

[System.Serializable]
public class ProductData
{
public string productName;
public Sprite productSprite;
public float basePrice; 

[Range(0, 100)]
public int currentDiscount = 0; // Скидка в процентах (задается кодом или вручную)

// Вычисление финальной цены с учетом скидки
public float GetFinalPrice()
{
if (currentDiscount <= 0) return basePrice;
return basePrice * (1f - (currentDiscount / 100f));
}

}