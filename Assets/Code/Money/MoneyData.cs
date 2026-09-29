using UnityEngine;

[CreateAssetMenu(fileName = "NewMoneyData", menuName = "Economy/Money Data")]
public class MoneyData : ScriptableObject
{
    public int nominal;
    public bool isCoin; 

    [Header("Спрайты состояний")]
    public Sprite spriteSingle;  
    public Sprite spriteDouble;
    public Sprite spriteSmallStack;
    public Sprite spriteBigStack;

    public Sprite GetSpriteForCount(int count)
    {
        if (count <= 0) return null;
        if (count == 1) return spriteSingle;
        if (count == 2) return spriteDouble;
        if (count > 2 && count <= 10) return spriteSmallStack;
        return spriteBigStack;
    }
}
