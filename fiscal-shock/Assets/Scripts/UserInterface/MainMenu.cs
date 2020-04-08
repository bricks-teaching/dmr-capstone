using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void Start() {
        Settings.forceUnlockCursorState();
        Settings.loadSettings();
        Application.targetFrameRate = Settings.targetFramerate;
        QualitySettings.vSyncCount = Settings.vsync;
    }

    void PlayClick() {
        Debug.Log("Starting game...");
        if (StateManager.sawEntryTutorial) {
            SceneManager.LoadScene("Hub");
        } else {
            SceneManager.LoadScene("Story");
        }
    }

    void QuitClick() {
        Debug.Log("Quitting from main menu.");
        Settings.saveSettings();
        Application.Quit();
    }
}
