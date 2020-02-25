using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start(){
        Debug.Log(SceneManager.GetActiveScene().buildIndex);
        Debug.Log(SceneManager.GetActiveScene().name);
    }

    public void PlayClick (){
        SceneManager.LoadScene("Hub");
    }

    public void QuitClick (){
        Debug.Log("Quit");
        Application.Quit();
    }
}
