using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script to control the main menu of the game.
/// </summary>
public class MainMenu : MonoBehaviour {
    public void Start() {
        Settings.forceUnlockCursorState();
        Settings.loadSettings();
        Application.targetFrameRate = Settings.targetFramerate;
        QualitySettings.vSyncCount = Settings.vsync;
    }

    /// <summary>
    /// Starts the game by loading the player into the hub.
    /// </summary>
    void PlayClick() {
        Debug.Log("Starting game...");
        SceneManager.LoadScene("Hub");
        Time.timeScale = 1;
    }

    /// <summary>
    /// Quits the game and closes the application.
    /// </summary>
    void QuitClick() {
        Debug.Log("Quitting from main menu.");
        Settings.saveSettings();
        Application.Quit();
    }

    
}
