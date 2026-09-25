using UnityEngine; 

public class RoomExitTrigger : MonoBehaviour
{
// Срабатывает автоматически, когда любой объект с коллайдером заходит внутрь этой зоны
private void OnTriggerEnter2D(Collider2D other)
{
// Проверяем, что зашедший объект — это наш главный герой (у него на префабе должно быть имя или мы ищем по компоненту движения)
// Так как префаб называется TestHero_0, при спавне его имя содержит "TestHero"
if (other.name.Contains("TestHero"))
{
// Находим менеджер игры и просим его открыть панель выбора локаций
GameFlowManager manager = FindObjectOfType<GameFlowManager>();
if (manager != null)
{
manager.OpenLocationPanel();
}
}
}
}