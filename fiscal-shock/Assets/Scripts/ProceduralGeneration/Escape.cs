using UnityEngine;

namespace FiscalShock.Procedural {
    public class Escape : MonoBehaviour {
        private Animation anim;
        private GameObject loadingScreen;
        private LoadingScreen loadScript;

        private void Start() {
            loadingScreen = GameObject.FindGameObjectWithTag("Loading Screen");
            loadScript = loadingScreen.GetComponent<LoadingScreen>();
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
                StateManager.selectedDungeon = (DungeonTypeEnum)(-1);
                GameObject player = collider.gameObject;
                SpawnPoint spawner = GameObject.FindGameObjectWithTag("Spawn Point").GetComponent<SpawnPoint>();

                // Disable shoot script, since player is entering town
                PlayerShoot shootScript = player.GetComponentInChildren<PlayerShoot>();
                shootScript.weapon?.SetActive(false);
                shootScript.crossHair.enabled = false;
                shootScript.enabled = false;
                player.GetComponentInChildren<Light>().intensity = 0;

                // Set player at the dungeon door
                spawner.transform.position = new Vector3(28, 1, -9);
                spawner.transform.LookAt(new Vector3(6, 1, -9));
                spawner.autoSpawn = false;
                spawner.spawnPlayer();

                // Manually kill the music box, since it isn't destroyed naturally
                GameObject musicPlayer = GameObject.Find("DungeonMusic");
                Destroy(musicPlayer);
                GameObject.Find("HUD").GetComponentInChildren<HUD>().escapeHatch = null;

                // Apply interest
                StateManager.startNewDay();
                loadScript.startLoadingScreen("Hub");
            }
        }
    }
}
