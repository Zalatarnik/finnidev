using UnityEngine;
using UnityEngine.SceneManagement;

public class AppBootstrapper : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("SampleScene"); 
    }
}
