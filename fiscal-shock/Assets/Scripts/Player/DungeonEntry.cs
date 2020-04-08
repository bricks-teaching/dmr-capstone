using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonEntry : MonoBehaviour {
    private bool isPlayerInTriggerZone = false;
    private GameObject loadingScreen;
    private GameObject player;
    private GameObject camera;
    private LoadingScreen loadScript;
    public Canvas textCanvas;
    public Canvas selectionScreen;

    void Start() {
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
        selectionScreen.enabled = false;

        camera = GameObject.Find("Main Camera");
        PlayerShoot shootScript = camera.GetComponent(typeof (PlayerShoot)) as PlayerShoot;
        if(shootScript.tutorial){
            PlayerFinance.cashOnHand = 1000.0f;
            shootScript.tutorial = false;
            shootScript.resetFeed();
            MouseLook lookScript = camera.GetComponent(typeof (MouseLook)) as MouseLook;
            lookScript.enabled = true;
            player = GameObject.Find("First Person Player");
            player.transform.position =  new Vector3(2.546f, 1f, -8.575f);
            player.transform.rotation = Quaternion.Euler(new Vector3(0f, 91f, 0f));
            PlayerHealth healthScript =  player.GetComponent(typeof (PlayerHealth)) as PlayerHealth;
            healthScript.resetVignette();
            PlayerMovement moveScript = player.GetComponent(typeof (PlayerMovement)) as PlayerMovement;
            moveScript.enabled = true;
        }
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

    public void selectDungeonStart(int value){
        loadScript.refreshStory();
        selectDungeon(value);
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
