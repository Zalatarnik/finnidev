using UnityEngine; 

public class RoomExitTrigger : MonoBehaviour
{
private void OnTriggerEnter2D(Collider2D other)
{

if (other.name.Contains("TestHero"))
{
GameFlowManager manager = FindObjectOfType<GameFlowManager>();
if (manager != null)
{
manager.OpenLocationPanel();
}
}
}
}