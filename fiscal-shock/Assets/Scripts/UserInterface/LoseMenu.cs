using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script to control the loss screen after the player runs out of money.
/// </summary>
public class LoseMenu : MonoBehaviour {
    private GameObject loadingScreen;
    private LoadingScreen loadScript;
    public void Start() {
        Settings.forceUnlockCursorState();
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
    }

    /// <summary>
    /// Loads the hub from the loss screen.
    /// </summary>
    public void RetryClick() {
        // Currently lets you "start over" where you left off, is this desirable?    
        loadScript.startLoadingScreen("Hub");
    }

    /// <summary>
    /// Saves the settings the user set and quits the game on click.
    /// </summary>
    public void BankruptClick() {
        Debug.Log("Quit");
        Settings.saveSettings();
        Application.Quit();
    }
}
