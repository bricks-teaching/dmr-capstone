using UnityEngine;

public class LoseMenu : MonoBehaviour {
    private GameObject loadingScreen;
    private LoadingScreen loadScript;

    public void Start() {
        Settings.forceUnlockCursorState();
        loadingScreen = GameObject.FindGameObjectWithTag("Loading Screen");
        loadScript = loadingScreen.GetComponent<LoadingScreen>();
    }

    public void RetryClick() {
        GameObject.FindGameObjectWithTag("Spawn Point").GetComponent<SpawnPoint>().resetToHubDefaults();
        loadScript.startLoadingScreen("Hub");
    }

    public void BankruptClick() {
        Debug.Log("Quit");
        Settings.quitToDesktop();
    }
}
