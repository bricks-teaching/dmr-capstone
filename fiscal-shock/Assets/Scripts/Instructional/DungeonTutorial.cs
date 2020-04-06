using UnityEngine;

public class DungeonTutorial : MonoBehaviour {
    void Start() {
        if (!StateManager.sawTutorial) {
            // Pause game
            Settings.mutexUnlockCursorState(this);
            Time.timeScale = 0;
        } else {
            GetComponent<Canvas>().enabled = false;
        }
    }

    public void Update() {
        if (Input.GetKeyDown(Settings.pauseKey)) {
            dismissWindow();
        }
    }

    public void dismissWindow() {
        GetComponent<Canvas>().enabled = false;
        Settings.lockCursorState(this);
        StateManager.sawTutorial = true;
        Time.timeScale = 1;
    }
}
