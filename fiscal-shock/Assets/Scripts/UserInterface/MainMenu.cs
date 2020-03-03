using UnityEngine;

public class MainMenu : MonoBehaviour {
    private GameObject loadingScreen;
    private LoadingScreen loadScript;

    public void Start() {
        Cursor.lockState = CursorLockMode.None;
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
    }

    void PlayClick() {
        Debug.Log("Starting game...");
        loadScript.startLoadingScreen("Hub");
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
