using UnityEngine;
using TMPro;

public class Sign : MonoBehaviour {
    private Canvas canvas;
    private TextMeshProUGUI signText;

    void Start() {
        canvas = GetComponentInChildren<Canvas>();
        canvas.enabled = false;
        signText = GetComponentInChildren<TextMeshProUGUI>();
        signText.text = signText.text.Replace("INTERACTKEY", Settings.interactKey.ToUpper()).Replace("PAUSEKEY", Settings.pauseKey.ToUpper());
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
