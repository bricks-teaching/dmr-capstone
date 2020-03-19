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
            anim = gameObject.GetComponentInChildren<Animation>();
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
                GameObject player = collider.gameObject;
                CharacterController playerController = player.GetComponentInChildren<CharacterController>();
                // Disable shoot script, since player is entering town
                player.GetComponentInChildren<PlayerShoot>().enabled = false;
                // Set player at the dungeon door
                playerController.enabled = false;
                player.transform.position = new Vector3(28, 1, -9);
                // Face player away from the door
                player.transform.LookAt(new Vector3(6, 1, -9));
                playerController.enabled = true;
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
