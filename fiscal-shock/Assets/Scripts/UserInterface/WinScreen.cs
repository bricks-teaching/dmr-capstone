using UnityEngine;

public class WinScreen : MonoBehaviour {
    public void Start() {
        Cursor.lockState = CursorLockMode.None;
    }

    public void QuitClick() {
        Debug.Log("Quit by win game.");
        Application.Quit();
    }
}
