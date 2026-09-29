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

        if (layoutElement != null) layoutElement.ignoreLayout = true; 

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

        if (transform.parent == canvas.transform)
        {
            transform.SetParent(startParent);
            if (layoutElement != null) layoutElement.ignoreLayout = false;
            rectTransform.anchoredPosition = startPosition;
        }
    }

    public void SuccessfullyDropped() 
    { 
        Destroy(gameObject); 
    }

    public Transform GetStartParent()
    {
        return startParent; 
    }
}
