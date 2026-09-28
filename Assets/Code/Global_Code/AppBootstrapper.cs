using UnityEngine;
using UnityEngine.SceneManagement;

public class AppBootstrapper : MonoBehaviour
{
    void Start()
    {
        // ВНИМАНИЕ: Здесь должно быть имя вашей главной сцены, 
        // где висит GameFlowManager, пролог и кастомизация!
        SceneManager.LoadScene("SampleScene"); 
    }
}
