using UnityEngine; 

[System.Serializable]
public class ProductData
{
public string productName;
public Sprite productSprite;
public float basePrice; 

[Range(0, 100)]
public int currentDiscount = 0;

public float GetFinalPrice()
{
if (currentDiscount <= 0) return basePrice;
return basePrice * (1f - (currentDiscount / 100f));
}

}