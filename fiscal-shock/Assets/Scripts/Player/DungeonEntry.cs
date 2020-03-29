using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonEntry : MonoBehaviour {
    private bool isPlayerInTriggerZone = false;
    private GameObject loadingScreen;
    private LoadingScreen loadScript;
    private Canvas canvas;

    void Start() {
        loadingScreen = GameObject.Find("LoadingScreen");
        loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
        canvas = GetComponentInChildren<Canvas>();
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
        if (isPlayerInTriggerZone && Input.GetKeyDown(Settings.interactKey) && (PlayerShoot.slotOne || PlayerShoot.slotZero) && PlayerFinance.totalDebt > 0.0f) { 
            canvas.enabled = false;
            StateManager.cashOnEntrance = PlayerFinance.cashOnHand;
            StateManager.timesEntered++;
            loadScript.startLoadingScreen("Dungeon");
        }
    }
}
