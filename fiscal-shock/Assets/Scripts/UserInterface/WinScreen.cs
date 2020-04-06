using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour {
    private GameObject loadingScreen;
    private LoadingScreen loadScript;
    public void Start() {
        Settings.forceUnlockCursorState();
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
    }

    public void QuitClick() {
        Debug.Log("Quit by win game.");
        Application.Quit();
    }

    public void GoBack() {
        loadScript.startLoadingScreen("Hub");
        Time.timeScale = 1;
        Settings.forceLockCursorState();
    }

}
