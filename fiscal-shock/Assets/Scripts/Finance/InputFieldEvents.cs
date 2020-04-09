using UnityEngine;
using TMPro;

public class InputFieldEvents : MonoBehaviour {
    private string originalText;
    public TextMeshProUGUI placeholder;

    private void Start() {
        originalText = placeholder.text;
    }

    public void hidePlaceholder() {
        placeholder.text = "";  // clear out the text
    }

    public void showPlaceholder() {
        placeholder.text = originalText;
    }
}
