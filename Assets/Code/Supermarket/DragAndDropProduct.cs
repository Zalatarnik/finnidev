using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class DragAndDropProduct : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
private Canvas canvas;
private RectTransform rectTransform;
private CanvasGroup canvasGroup;
private Vector2 anchoredStartPosition; 

[HideInInspector] public ProductData currentProduct; // Данные о конкретном товаре

void Awake()
{
rectTransform = GetComponent<RectTransform>();
canvas = GetComponentInParent<Canvas>();
// Добавляем CanvasGroup программно, чтобы товар не блокировал обнаружение стола
canvasGroup = GetComponent<CanvasGroup>();
if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

}

void Start()
{
anchoredStartPosition = rectTransform.anchoredPosition;
}

public void OnBeginDrag(PointerEventData eventData)
{
canvasGroup.alpha = 0.6f;
canvasGroup.blocksRaycasts = false; // "Смотрим" сквозь летящий товар
}

public void OnDrag(PointerEventData eventData)
{
rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; // Тащим за пальцем
}

public void OnEndDrag(PointerEventData eventData)
{
canvasGroup.alpha = 1f;
canvasGroup.blocksRaycasts = true;
// Если товар бросили на объект с именем CounterTable (наш прилавок)
if (eventData.pointerCurrentRaycast.gameObject != null && 
    eventData.pointerCurrentRaycast.gameObject.name == "CounterTable")
{
    // Отправляем цену на кассу
    SupermarketManager.Instance.AddProductToCart(currentProduct);
    gameObject.SetActive(false); // Товар упал на стол, убираем с полки
}
else
{
    rectTransform.anchoredPosition = anchoredStartPosition; // Мимо стола — летит назад на полку
}

}

public void ResetToShelf()
{
gameObject.SetActive(true);
rectTransform.anchoredPosition = anchoredStartPosition;
}

}