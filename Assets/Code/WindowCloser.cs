using UnityEngine;


public class WindowCloser : MonoBehaviour
{
    public void CloseMyWindow()
    {
        gameObject.SetActive(false);
    }
}