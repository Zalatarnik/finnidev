using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class DragAndDropProduct : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 anchoredStartPosition; 

    [HideInInspector] public ProductData currentProduct; 

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
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
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (eventData.pointerCurrentRaycast.gameObject != null && 
            eventData.pointerCurrentRaycast.gameObject.name == "CounterTable")
        {
            rectTransform.anchoredPosition = anchoredStartPosition;
        }
        else
        {
            rectTransform.anchoredPosition = anchoredStartPosition; 
        }
    }

    public void ResetToShelf()
    {
        gameObject.SetActive(true);
        rectTransform.anchoredPosition = anchoredStartPosition;
    }
}