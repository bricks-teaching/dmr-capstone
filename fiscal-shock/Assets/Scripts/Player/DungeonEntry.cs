using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonEntry : MonoBehaviour {
    private bool isPlayerInTriggerZone = false;
    private GameObject loadingScreen;
    private LoadingScreen loadScript;
    public Canvas textCanvas;
    public Canvas selectionScreen;
    public AudioClip bummer;
    public TextMeshProUGUI texto;
    private string originalText;
    private AudioSource audioSource;

    void Start() {
        Time.timeScale = 1;  // sorry but it won't restart in the hub rightly
        loadingScreen = GameObject.FindGameObjectWithTag("Loading Screen");
        loadScript = loadingScreen.GetComponent<LoadingScreen>();
        selectionScreen.enabled = false;
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            isPlayerInTriggerZone = true;
            originalText = texto.text;
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Player") {
            isPlayerInTriggerZone = false;
            texto.text = originalText;
        }
    }

    void FixedUpdate() {
        if (isPlayerInTriggerZone) {
            if (Input.GetKeyDown(Settings.interactKey)) {
                if (GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerShoot>().guns.Count < 1) {
                    audioSource.PlayOneShot(bummer, Settings.volume);
                    texto.text = "It's dangerous to go out alone (and unarmed).";
                    return;
                }
                StateManager.pauseAvailable = false;
                Settings.forceUnlockCursorState();
                textCanvas.enabled = false;
                selectionScreen.enabled = true;
                StateManager.pauseAvailable = false;
            }
            if (Input.GetKeyDown(Settings.pauseKey)) {
                closeSelectionScreen();
            }
        }
    }

    public void closeSelectionScreen() {
        selectionScreen.enabled = false;
        Settings.mutexLockCursorState(this);
        StateManager.pauseAvailable = true;
        if (isPlayerInTriggerZone) {
            textCanvas.enabled = true;
        }
    }

    public void selectDungeonStart(int value){
        selectDungeon(value);
    }

    public void selectDungeon(int value) {
        selectionScreen.enabled = false;
        StateManager.selectedDungeon = (DungeonTypeEnum)value;
        StateManager.cashOnEntrance = StateManager.cashOnHand;
        StateManager.timesEntered++;
        StateManager.totalFloorsVisited++;
        StateManager.currentFloor = 1;
        StateManager.startedFromDungeon = false;
        Settings.forceLockCursorState();
        StartCoroutine(StateManager.makePauseAvailableAgain());
        loadScript.startLoadingScreen("Dungeon");
    }
}
