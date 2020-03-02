using UnityEngine;

public class Sign : MonoBehaviour {
    private Canvas canvas;

    void Start(){
        Debug.Log($"{gameObject.name}: Starting sign");
        canvas = GetComponentInChildren<Canvas>();
        canvas.enabled = false;
    }

    void OnTriggerEnter(){
        Debug.Log($"{gameObject.name}: Triggered");
        canvas.enabled = true;
    }

    void OnTriggerExit(){
        Debug.Log($"{gameObject.name}: Left trigger zone");
        canvas.enabled = false;
    }
}
