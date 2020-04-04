using UnityEngine;

public class LoseMenu : MonoBehaviour {
    private GameObject loadingScreen;
    private LoadingScreen loadScript;
    // TODO use spawn point gameobject system instead to always put player where they should go
    private Quaternion hubRotation = Quaternion.Euler(0, 90, 0);
    private Vector3 hubSpawnPoint = new Vector3(3.117362f, 1.2f, -7.210602f);

    public void Start() {
        Settings.forceUnlockCursorState();
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
    }

    public void RetryClick() {
        // Currently lets you "start over" where you left off, is this desirable?
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.rotation = hubRotation;
        player.transform.position = hubSpawnPoint;
        loadScript.startLoadingScreen("Hub");
    }

    public void BankruptClick() {
        Debug.Log("Quit");
        Settings.quitToDesktop();
    }
}
