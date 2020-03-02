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
            if (collider.gameObject.tag == "Player") {
                loadScript.startLoadingScreen("Hub");
                // Manually kill the music box, since it isn't destroyed naturally
                GameObject musicPlayer = GameObject.Find("DungeonMusic");
                Destroy(musicPlayer);
                // Apply interest
                PlayerFinance.startNewDay();
            }
        }
    }
}
