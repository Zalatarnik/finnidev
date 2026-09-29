using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WorldCameraController : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private Transform sc_1;

    [Header("Movement")]
    [SerializeField] private float mouseDragSpeed = 1f;
    [SerializeField] private float touchDragSpeed = 1f;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 8f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float pinchZoomSpeed = 0.01f;

    [Header("Smoothing")]
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float smoothSpeed = 15f;

    private Camera cam;
    private Bounds worldBounds;
    private Vector3 targetPosition;
    private float targetZoom;

    private bool isMousePanning;
    private Vector2 lastMousePosition;

    private int activeTouchId = -1;
    private Vector2 lastTouchPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
            cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("WorldCameraController: Camera не найдена.");
            enabled = false;
            return;
        }

        if (!cam.orthographic)
            Debug.LogWarning("WorldCameraController: Camera должна быть Orthographic.");

        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;

        FindBounds();
        ClampZoom();
        ClampCamera();
        ApplyCameraPosition();
    }

    private void Update()
    {
        if (cam == null || sc_1 == null)
            return;

        FindBounds();

        HandleMouse();
        HandleTouch();
        HandleZoom();

        ClampZoom();
        ClampCamera();
        ApplyCameraPosition();
    }

    private void HandleMouse()
    {
        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (!IsScreenPositionValid(mousePosition))
        {
            isMousePanning = false;
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (IsPointerOverDraggableObject(mousePosition))
            {
                isMousePanning = false;
                return;
            }

            isMousePanning = true;
            lastMousePosition = mousePosition;
        }

        if (Mouse.current.leftButton.isPressed && isMousePanning)
        {
            Vector3 previousWorld = ScreenToWorld(lastMousePosition);
            Vector3 currentWorld = ScreenToWorld(mousePosition);

            Vector3 delta = previousWorld - currentWorld;
            delta.z = 0f;

            targetPosition += delta * mouseDragSpeed;
            lastMousePosition = mousePosition;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            isMousePanning = false;
    }

    private void HandleTouch()
    {
        if (Touchscreen.current == null)
            return;

        var touches = Touchscreen.current.touches;

        int firstTouchIndex = -1;
        int secondTouchIndex = -1;

        for (int i = 0; i < touches.Count; i++)
        {
            if (!touches[i].press.isPressed)
                continue;

            if (firstTouchIndex == -1)
                firstTouchIndex = i;
            else
            {
                secondTouchIndex = i;
                break;
            }
        }

        if (firstTouchIndex != -1 && secondTouchIndex != -1)
        {
            activeTouchId = -1;

            var touch0 = touches[firstTouchIndex];
            var touch1 = touches[secondTouchIndex];

            Vector2 current0 = touch0.position.ReadValue();
            Vector2 current1 = touch1.position.ReadValue();

            Vector2 previous0 = current0 - touch0.delta.ReadValue();
            Vector2 previous1 = current1 - touch1.delta.ReadValue();

            float currentDistance = Vector2.Distance(current0, current1);
            float previousDistance = Vector2.Distance(previous0, previous1);

            float difference = currentDistance - previousDistance;

            if (Mathf.Abs(difference) > 0.01f)
            {
                ZoomAroundScreenPoint(
                    (current0 + current1) * 0.5f,
                    -difference * pinchZoomSpeed
                );
            }

            return;
        }

        if (firstTouchIndex == -1)
        {
            activeTouchId = -1;
            return;
        }

        var touch = touches[firstTouchIndex];
        int touchId = touch.touchId.ReadValue();
        Vector2 touchPosition = touch.position.ReadValue();

        if (touch.press.wasPressedThisFrame)
        {
            if (IsPointerOverDraggableObject(touchPosition))
            {
                activeTouchId = -1;
                return;
            }

            activeTouchId = touchId;
            lastTouchPosition = touchPosition;
        }

        if (touchId != activeTouchId)
            return;

        if (touch.press.isPressed)
        {
            Vector2 delta = touch.delta.ReadValue();

            if (delta.sqrMagnitude > 0.0001f)
            {
                Vector3 previousWorld = ScreenToWorld(lastTouchPosition);
                Vector3 currentWorld = ScreenToWorld(touchPosition);

                Vector3 worldDelta = previousWorld - currentWorld;
                worldDelta.z = 0f;

                targetPosition += worldDelta * touchDragSpeed;
                lastTouchPosition = touchPosition;
            }
        }

        if (touch.press.wasReleasedThisFrame)
            activeTouchId = -1;
    }

    private void HandleZoom()
    {
        if (Mouse.current == null)
            return;

        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (Mathf.Abs(scroll.y) > 0.01f)
        {
            ZoomAroundScreenPoint(
                Mouse.current.position.ReadValue(),
                -scroll.y * 0.01f * zoomSpeed
            );
        }
    }

    private void ZoomAroundScreenPoint(Vector2 screenPoint, float zoomDelta)
    {
        float oldZoom = targetZoom;

        Vector3 worldBeforeZoom = ScreenToWorldWithZoom(screenPoint, oldZoom);

        targetZoom += zoomDelta;
        ClampZoom();

        if (Mathf.Approximately(oldZoom, targetZoom))
            return;

        Vector3 worldAfterZoom = ScreenToWorldWithZoom(screenPoint, targetZoom);

        targetPosition += worldBeforeZoom - worldAfterZoom;
        ClampCamera();
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        return ScreenToWorldWithZoom(screenPosition, targetZoom);
    }

    private Vector3 ScreenToWorldWithZoom(Vector2 screenPosition, float zoom)
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return targetPosition;

        float normalizedX = screenPosition.x / Screen.width - 0.5f;
        float normalizedY = screenPosition.y / Screen.height - 0.5f;

        float halfHeight = zoom;
        float halfWidth = zoom * cam.aspect;

        return new Vector3(
            targetPosition.x + normalizedX * halfWidth * 2f,
            targetPosition.y + normalizedY * halfHeight * 2f,
            transform.position.z
        );
    }

    private void FindBounds()
    {
        if (sc_1 == null)
            return;

        Collider2D collider = sc_1.GetComponent<Collider2D>();

        if (collider != null)
        {
            worldBounds = collider.bounds;
            return;
        }

        SpriteRenderer spriteRenderer = sc_1.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            worldBounds = spriteRenderer.bounds;
            return;
        }
    }

    private void ClampZoom()
    {
        if (worldBounds.size == Vector3.zero)
            return;

        float maxZoomByHeight = worldBounds.extents.y;
        float maxZoomByWidth = worldBounds.extents.x / cam.aspect;

        float allowedMaxZoom = Mathf.Min(
            maxZoom,
            maxZoomByHeight,
            maxZoomByWidth
        );

        float actualMinZoom = Mathf.Min(minZoom, allowedMaxZoom);

        targetZoom = Mathf.Clamp(
            targetZoom,
            actualMinZoom,
            allowedMaxZoom
        );
    }

    private void ClampCamera()
    {
        if (worldBounds.size == Vector3.zero)
            return;

        float halfHeight = targetZoom;
        float halfWidth = targetZoom * cam.aspect;

        float minX = worldBounds.min.x + halfWidth;
        float maxX = worldBounds.max.x - halfWidth;

        float minY = worldBounds.min.y + halfHeight;
        float maxY = worldBounds.max.y - halfHeight;

        if (minX <= maxX)
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        else
            targetPosition.x = worldBounds.center.x;

        if (minY <= maxY)
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        else
            targetPosition.y = worldBounds.center.y;

        targetPosition.z = transform.position.z;
    }

    private void ApplyCameraPosition()
    {
        cam.orthographicSize = targetZoom;

        if (smoothMovement)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                1f - Mathf.Exp(-smoothSpeed * Time.deltaTime)
            );
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    private bool IsScreenPositionValid(Vector2 position)
    {
        return position.x >= 0f &&
               position.x <= Screen.width &&
               position.y >= 0f &&
               position.y <= Screen.height;
    }

    private bool IsPointerOverDraggableObject(Vector2 screenPosition)
    {
        if (!IsScreenPositionValid(screenPosition))
            return false;

        Vector3 world = ScreenToWorld(screenPosition);

        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(world.x, world.y),
            Vector2.zero
        );

        if (hit.collider == null)
            return false;

        PhysicsDragAndDrop drag =
            hit.collider.GetComponentInParent<PhysicsDragAndDrop>();

        return drag != null;
    }
}
