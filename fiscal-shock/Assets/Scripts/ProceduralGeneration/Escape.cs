using UnityEngine;

namespace FiscalShock.Procedural {
    public class Escape : MonoBehaviour {
        private Animation anim;
        private GameObject loadingScreen;
        private LoadingScreen loadScript;

        private void Start() {
            loadingScreen = GameObject.Find("LoadingScreen");
            loadScript = (LoadingScreen)loadingScreen.GetComponent<LoadingScreen>();
            // Play the arrow spinning animation
            anim = gameObject.GetComponent<Animation>();
            anim["Spin"].speed = 0.3f;
        }

        private void Update() {
            if (anim.isPlaying) {
                return;
            }
            anim.Play("Spin");
        }

        private void OnTriggerEnter(Collider collider) {
            Debug.Log("Something entered the escape hatch!");
            if (collider.gameObject.tag == "Player") {
                loadScript.startLoadingScreen("Hub");
                // Maybe hit the "NewDay" script here?
            }
        }
    }
}
