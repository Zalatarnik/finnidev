using UnityEngine; 

public class CameraFollow : MonoBehaviour
{
private Transform target; // Сюда код сам подставит персонажа после его спавна
public float smoothSpeed = 5f; // Скорость плавного сглаживания камеры
public Vector3 offset = new Vector3(0, 0, -10f); // Смещение камеры (Z должно быть -10) 

public void SetTarget(Transform newTarget)
{
target = newTarget;
}

void LateUpdate()
{
// Если персонаж уже создан и появился на сцене
if (target != null)
{
// Вычисляем идеальную точку, где должна быть камера
Vector3 desiredPosition = target.position + offset;
    // Плавно передвигаем камеру из текущей позиции в идеальную
    Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    
    // Обновляем позицию камеры
    transform.position = smoothedPosition;
}

}

}