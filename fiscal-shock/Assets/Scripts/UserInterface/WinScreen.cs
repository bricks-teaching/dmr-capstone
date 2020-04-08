using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script for controlling the button on the win screen.
/// </summary>
public class WinScreen : MonoBehaviour {
    private GameObject loadingScreen;
    private LoadingScreen loadScript;
    public void Start() {
        Settings.forceUnlockCursorState();
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
    }

    /// <summary>
    /// Closes the game and quits.
    /// </summary>
    public void QuitClick() {
        Debug.Log("Quit by win game.");
        Application.Quit();
    }

    /// <summary>
    /// Bravely puts the player back into the hub to continue earning.
    /// </summary>
    public void GoBack() {
        loadScript.startLoadingScreen("Hub");
        Time.timeScale = 1;
        Settings.forceLockCursorState();
    }

}
