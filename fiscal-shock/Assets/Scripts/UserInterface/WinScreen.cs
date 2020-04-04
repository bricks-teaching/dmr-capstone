using UnityEngine;

public class WinScreen : MonoBehaviour {
    public void Start() {
        Settings.forceUnlockCursorState();
    }

    public void QuitClick() {
        Debug.Log("Quit by win game.");
        Settings.quitToDesktop();
    }
}
