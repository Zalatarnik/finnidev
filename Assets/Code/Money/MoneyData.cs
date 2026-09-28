using UnityEngine;

[CreateAssetMenu(fileName = "NewMoneyData", menuName = "Economy/Money Data")]
public class MoneyData : ScriptableObject
{
    public int nominal; // Например: 50, 100, 500, 1000, 2000, 5000 или 1, 2, 5, 10
    public bool isCoin; // Галочка, если это монета (для логики физики)

    [Header("Спрайты состояний")]
    public Sprite spriteSingle;     // 1 шт
    public Sprite spriteDouble;     // 2 шт
    public Sprite spriteSmallStack; // 3-10 шт
    public Sprite spriteBigStack;   // >10 шт

    // Метод автоматически возвращает нужный спрайт в зависимости от количества
    public Sprite GetSpriteForCount(int count)
    {
        if (count <= 0) return null;
        if (count == 1) return spriteSingle;
        if (count == 2) return spriteDouble;
        if (count > 2 && count <= 10) return spriteSmallStack;
        return spriteBigStack; // Больше 10
    }
}
