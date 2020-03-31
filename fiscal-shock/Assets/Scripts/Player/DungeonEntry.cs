using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonEntry : MonoBehaviour {
    private bool isPlayerInTriggerZone = false;
    private GameObject loadingScreen;
    private LoadingScreen loadScript;
    public Canvas textCanvas;
    public Canvas selectionScreen;

    void Start() {
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
        selectionScreen.enabled = false;
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            isPlayerInTriggerZone = true;
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Player") {
            isPlayerInTriggerZone = false;
        }
    }

    void FixedUpdate() {
        if (isPlayerInTriggerZone && Input.GetKeyDown(Settings.interactKey)) {
            Settings.forceUnlockCursorState();
            textCanvas.enabled = false;
            selectionScreen.enabled = true;
        }
    }

    public void closeSelectionScreen() {
        selectionScreen.enabled = false;
        Settings.mutexLockCursorState(this);
        if (isPlayerInTriggerZone) {
            textCanvas.enabled = true;
        }
    }

    public void selectDungeon(int value) {
        selectionScreen.enabled = false;
        StateManager.selectedDungeon = (DungeonTypeEnum)value;
        StateManager.cashOnEntrance = PlayerFinance.cashOnHand;
        StateManager.timesEntered++;
        Settings.forceLockCursorState();
        loadScript.startLoadingScreen("Dungeon");
    }
}
