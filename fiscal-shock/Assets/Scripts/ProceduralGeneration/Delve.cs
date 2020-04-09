using UnityEngine;

namespace FiscalShock.Procedural {
    public class Delve : MonoBehaviour {
        private GameObject loadingScreen;
        private LoadingScreen loadScript;
        private Animation anim;

        private void Start() {
            loadingScreen = GameObject.FindGameObjectWithTag("Loading Screen");
            loadScript = loadingScreen.GetComponent<LoadingScreen>();
            // Play the arrow spinning animation
            anim = gameObject.GetComponentInChildren<Animation>();
            anim["Delve"].speed = 0.8f;
        }

        private void Update() {
            if (anim.isPlaying) {
                return;
            }
            anim.Play("Delve");
        }

        private void OnTriggerEnter(Collider collider) {
            if (collider.gameObject.tag == "Player") {
                loadScript.startLoadingScreen("Dungeon");
            }
        }
    }
}
