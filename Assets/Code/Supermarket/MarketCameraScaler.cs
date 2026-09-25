using UnityEngine; 

public class MarketCameraScaler : MonoBehaviour
{
[Header("Settings")]
public SpriteRenderer marketBackground; // Сюда перетащим картинку магазина с продавцом 

void Start()
{
Camera cam = GetComponent<Camera>();
if (cam == null || marketBackground == null)
{
    Debug.LogError("Забыл прикрепить камеру или спрайт фона в инспекторе!");
    return;
}

// 1. Сначала подгоняем размер камеры (Size), чтобы картинка магазина идеально влезала по высоте экрана
float bgHeight = marketBackground.bounds.size.y;
cam.orthographicSize = bgHeight / 2f;

// 2. Вычисляем, какую ширину видит камера на конкретном экране телефона
float camHorizontalExtent = cam.orthographicSize * cam.aspect;

// 3. Находим, где в игровом мире находится самый правый край картинки магазина
float bgRightEdge = marketBackground.bounds.max.x;

// 4. Сдвигаем камеру по оси X ровно настолько, чтобы её правый край совпал с правым краем картинки
float targetCameraX = bgRightEdge - camHorizontalExtent;

// Фиксируем камеру в этой точке (высоту Y берем из центра картинки)
float targetCameraY = marketBackground.bounds.center.y;
transform.position = new Vector3(targetCameraX, targetCameraY, -10f);

}

}