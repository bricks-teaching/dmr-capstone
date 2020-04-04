using UnityEngine;
using TMPro;

public class Sign : MonoBehaviour {
    public Canvas canvas;
    public TextMeshProUGUI signText;

    void Start() {
        if (canvas == null) {
            canvas = GetComponentInChildren<Canvas>();
        }
        canvas.enabled = false;
        if (signText == null) {
            signText = GetComponentInChildren<TextMeshProUGUI>();
        }
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
