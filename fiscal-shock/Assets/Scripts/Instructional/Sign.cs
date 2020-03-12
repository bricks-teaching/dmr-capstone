using UnityEngine;

public class Sign : MonoBehaviour {
    private Canvas canvas;

    void Start() {
        canvas = GetComponentInChildren<Canvas>();
        canvas.enabled = false;
    }

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Player")
        {
            canvas.enabled = true;
        }
    }

    void OnTriggerExit(Collider col) {
        if(col.gameObject.tag == "Player")
        {
            canvas.enabled = false;
        }
    }
}
