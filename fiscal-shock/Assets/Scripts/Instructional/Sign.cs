using UnityEngine;

public class Sign : MonoBehaviour {
    private Canvas canvas;

    void Start() {
        canvas = GetComponentInChildren<Canvas>();
        canvas.enabled = false;
    }

    void OnTriggerEnter() {
        canvas.enabled = true;
    }

    void OnTriggerExit() {
        canvas.enabled = false;
    }
}
