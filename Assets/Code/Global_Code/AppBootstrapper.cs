using UnityEngine;
using UnityEngine.SceneManagement;

public class AppBootstrapper : MonoBehaviour
{
    void Start()
    {
        // ВНИМАНИЕ: Напишите сюда точное имя сцены, где выбирают персонажа (как в иерархии)
        SceneManager.LoadScene("SampleScene"); 
    }
}
