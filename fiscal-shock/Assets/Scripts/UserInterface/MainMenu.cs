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
        SceneManager.LoadScene("Hub");
        Time.timeScale = 1;
    }

    void QuitClick() {
        Debug.Log("Quitting from main menu.");
        Settings.saveSettings();
        Application.Quit();
    }

    
}
