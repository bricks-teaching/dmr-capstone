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

    void OnGUI() {
    // Temporary graphics settings shamelessly stolen from unity docs
        string[] names = QualitySettings.names;
        GUILayout.BeginVertical();
        for (int i = 0; i < names.Length; i++) {
            if (GUILayout.Button(names[i])) {
                QualitySettings.SetQualityLevel(i, true);
            }
        }
        GUILayout.EndVertical();
    }
}
