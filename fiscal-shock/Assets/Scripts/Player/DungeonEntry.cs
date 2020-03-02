using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonEntry : MonoBehaviour {
    private bool isPlayerInTriggerZone = false;

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            isPlayerInTriggerZone = true;
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Player") {
            isPlayerInTriggerZone = false;
        }
    }

    void FixedUpdate() {
        if (isPlayerInTriggerZone && Input.GetKeyDown("x")) {
            // TODO use loading screen instead - not available on this branch yet
            SceneManager.LoadScene("Dungeon");
        }
    }
}
