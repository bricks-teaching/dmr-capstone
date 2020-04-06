using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseMenu : MonoBehaviour {
    private GameObject loadingScreen;
    private LoadingScreen loadScript;
    public void Start() {
        Settings.forceUnlockCursorState();
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
    }

    public void RetryClick() {
        // Currently lets you "start over" where you left off, is this desirable?    
        loadScript.startLoadingScreen("Hub");
    }

    public void BankruptClick() {
        Debug.Log("Quit");
        Settings.saveSettings();
        Application.Quit();
    }
}
