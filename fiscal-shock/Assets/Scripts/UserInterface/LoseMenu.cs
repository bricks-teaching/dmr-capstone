using UnityEngine;

/// <summary>
/// Script to control the loss screen after the player runs out of money.
/// </summary>
public class LoseMenu : MonoBehaviour {
    private GameObject loadingScreen;
    private LoadingScreen loadScript;

    public void Start() {
        Settings.forceUnlockCursorState();
        loadingScreen = GameObject.FindGameObjectWithTag("Loading Screen");
        loadScript = loadingScreen.GetComponent<LoadingScreen>();
    }

    /// <summary>
    /// Loads the hub from the loss screen.
    /// </summary>
    public void RetryClick() {
        GameObject.FindGameObjectWithTag("Spawn Point").GetComponent<SpawnPoint>().resetToHubDefaults();
        StateManager.playerDead = false;
        loadScript.startLoadingScreen("Hub");
    }

    /// <summary>
    /// Saves the settings the user set and quits the game on click.
    /// </summary>
    public void BankruptClick() {
        Debug.Log("Quit");
        StateManager.playerDead = false;
        Settings.quitToDesktop();
    }
}
