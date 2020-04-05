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
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
        selectionScreen.enabled = false;
        originalText = texto.text;
        audioSource = GameObject.FindObjectOfType<AudioSource>();
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            isPlayerInTriggerZone = true;
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Player") {
            isPlayerInTriggerZone = false;
            texto.text = originalText;
        }
    }

    void FixedUpdate() {
        if (isPlayerInTriggerZone && Input.GetKeyDown(Settings.interactKey)) {
            if(!(StateManager.purchasedHose || StateManager.purchasedLauncher) ) {
                audioSource.PlayOneShot(bummer, Settings.volume * 3f);
                texto.text = "It's dangerous to go out alone (and unarmed).";
                return;
            }
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
