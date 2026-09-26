using UnityEngine;
using UnityEngine.EventSystems;

public class PhysicsDragAndDrop : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public ProductData currentProduct;

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Camera mainCam;
    private bool isDragging = false;

    [HideInInspector] public bool isOnCounter = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogError(
                "PhysicsDragAndDrop: Main Camera не найдена. " +
                "Убедись, что у камеры установлен Tag = MainCamera."
            );
        }
    }

    public void SetStartPosition(Vector3 position)
    {
        startPosition = position;

        if (rb != null)
            rb.position = new Vector2(position.x, position.y);

        transform.position = position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (SupermarketManager.Instance != null &&
            SupermarketManager.Instance.isSelectionLocked)
        {
            return;
        }

        isDragging = true;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam == null)
            return;

        // Луч из камеры через позицию мыши/пальца.
        Ray ray = mainCam.ScreenPointToRay(eventData.position);

        // Плоскость, на которой находится 2D-товар.
        Plane dragPlane = new Plane(
            Vector3.forward,
            new Vector3(0f, 0f, transform.position.z)
        );

        if (!dragPlane.Raycast(ray, out float distance))
            return;

        Vector3 worldPosition = ray.GetPoint(distance);

        // Для 2D сохраняем исходный Z.
        worldPosition.z = transform.position.z;

        if (rb != null)
            rb.position = new Vector2(worldPosition.x, worldPosition.y);

        transform.position = worldPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        if (rb != null)
            rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "CounterTable")
        {
            if (SupermarketManager.Instance != null)
                SupermarketManager.Instance.HandleProductDroppedOnCounter(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "FloorTrigger")
        {
            if (isOnCounter)
            {
                if (SupermarketManager.Instance != null)
                    SupermarketManager.Instance.RemoveCounterProduct(this);

                Destroy(gameObject);
                return;
            }

            ResetToShelfPosition();
        }
    }

    public void ResetToShelfPosition()
    {
        isDragging = false;
        isOnCounter = false;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = new Vector2(startPosition.x, startPosition.y);
        }

        transform.rotation = Quaternion.identity;
        transform.position = startPosition;
    }

    private void Update()
    {
        if (isDragging)
            return;

        if (transform.position.y < -50f ||
            Mathf.Abs(transform.position.x) > 50f)
        {
            if (isOnCounter)
            {
                if (SupermarketManager.Instance != null)
                    SupermarketManager.Instance.RemoveCounterProduct(this);

                Destroy(gameObject);
            }
            else
            {
                ResetToShelfPosition();
            }
        }
    }
}
