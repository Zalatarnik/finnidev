using UnityEngine;
using UnityEngine.EventSystems; 

public class DynamicJoystickMovement : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
[Header("Joystick UI Elements")]
public RectTransform joystickBackground;
public RectTransform joystickHandle; 

[Header("Movement Settings")]
public float moveSpeed = 5f;

private Transform playerTransform;
private Rigidbody2D playerRigidbody;
private Vector2 inputVector;
private Vector2 joystickCenter;
private float joystickRadius;

void Start()
{
if (joystickBackground != null)
{
joystickBackground.gameObject.SetActive(false);
joystickRadius = joystickBackground.sizeDelta.x / 2f;
}
}

public void SetTarget(Transform target)
{
playerTransform = target;
if (target != null)
{
playerRigidbody = target.GetComponent<Rigidbody2D>();
}
}

public void OnPointerDown(PointerEventData eventData)
{
if (joystickBackground == null) return;
joystickBackground.gameObject.SetActive(true);
joystickBackground.position = eventData.position;
joystickCenter = eventData.position;
joystickHandle.anchoredPosition = Vector2.zero;

}

public void OnDrag(PointerEventData eventData)
{
if (joystickBackground == null) return;
Vector2 direction = eventData.position - joystickCenter;
inputVector = direction.magnitude > joystickRadius ? direction.normalized : direction / joystickRadius;
joystickHandle.anchoredPosition = inputVector * joystickRadius;

}

public void OnPointerUp(PointerEventData eventData)
{
if (joystickBackground == null) return;
inputVector = Vector2.zero;
joystickHandle.anchoredPosition = Vector2.zero;
joystickBackground.gameObject.SetActive(false);

if (playerRigidbody != null)
{
    playerRigidbody.linearVelocity = Vector2.zero;
}

}

void FixedUpdate()
{
if (playerRigidbody != null && inputVector != Vector2.zero)
{
Vector2 velocity = inputVector * moveSpeed;
playerRigidbody.linearVelocity = velocity;
    if (inputVector.x > 0)
    {
        playerTransform.localScale = new Vector3(Mathf.Abs(playerTransform.localScale.x), playerTransform.localScale.y, playerTransform.localScale.z);
    }
    else if (inputVector.x < 0)
    {
        playerTransform.localScale = new Vector3(-Mathf.Abs(playerTransform.localScale.x), playerTransform.localScale.y, playerTransform.localScale.z);
    }
}
else if (playerRigidbody != null && inputVector == Vector2.zero)
{
    playerRigidbody.linearVelocity = Vector2.zero;
}

}

}