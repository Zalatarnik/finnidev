using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableMoneyItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public MoneyData moneyData; 
    
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private LayoutElement layoutElement; 
    
    private Vector2 startPosition;
    private Transform startParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Автоматически добавляем LayoutElement, если его забыли поставить в Инспекторе
        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = gameObject.AddComponent<LayoutElement>();
        
        canvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        
        // Говорим Grid Layout Group временно игнорировать этот объект, чтобы мы могли его двигать
        if (layoutElement != null) layoutElement.ignoreLayout = true; 
        
        // Выносим купюру на самый верхний слой экрана во время перетаскивания
        transform.SetParent(canvas.transform); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Если купюру не поймала ни одна из свинок (она осталась дочерней к Canvas)
        if (transform.parent == canvas.transform)
        {
            // Возвращаем её обратно в её родную ячейку Scroll View или на стол мамы
            transform.SetParent(startParent);
            if (layoutElement != null) layoutElement.ignoreLayout = false; // Возвращаем под контроль сетки
            rectTransform.anchoredPosition = startPosition;
        }
    }

    public void SuccessfullyDropped() 
    { 
        Destroy(gameObject); 
    }

    // =========================================================================
    // ДОБАВЛЕННЫЙ МЕТОД:
    // Позволяет зонам сброса (копилкам) точно узнать, где купюра лежала до начала драга
    // =========================================================================
    public Transform GetStartParent()
    {
        return startParent; 
    }
}
