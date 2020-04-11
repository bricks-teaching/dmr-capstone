using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void Start() {
        Settings.forceUnlockCursorState();
        Settings.loadSettings();
    }

    void PlayClick() {
        Debug.Log("Starting game...");
        if (Settings.values.sawStoryTutorial) {
            SceneManager.LoadScene("Hub");
        } else {
            SceneManager.LoadScene("Story");
        }
    }

    void QuitClick() {
        Debug.Log("Quitting from main menu.");
        Settings.quitToDesktop();
    }
}
