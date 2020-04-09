using UnityEngine;

public class DungeonTutorial : MonoBehaviour {
    void Start() {
        if (!StateManager.sawEntryTutorial) {
            // Pause game
            Settings.mutexUnlockCursorState(this);
            Time.timeScale = 0;
        } else {
            GetComponent<Canvas>().enabled = false;
        }
    }

    public void Update() {
        if (!StateManager.sawEntryTutorial) {
            Time.timeScale = 0;  // failsafe due to async in load screen
        }
        if (Input.GetKeyDown(Settings.pauseKey)) {
            dismissWindow();
        }
    }

    public void dismissWindow() {
        GetComponent<Canvas>().enabled = false;
        Settings.lockCursorState(this);
        StateManager.sawEntryTutorial = true;
        Time.timeScale = 1;
    }
}
