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
