using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    void PlayClick() {
        Debug.Log("Starting game...");
        SceneManager.LoadScene("Hub");
    }

    void QuitClick() {
        Debug.Log("Quitting from main menu.");
        Application.Quit();
    }
}
