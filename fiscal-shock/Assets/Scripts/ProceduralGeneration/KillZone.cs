using UnityEngine;

namespace FiscalShock.Procedural {
    public class KillZone : MonoBehaviour {
        private GameObject loadingScreen;
        private LoadingScreen loadScript;

        private void Start() {
            loadingScreen = GameObject.FindGameObjectWithTag("Loading Screen");
            loadScript = loadingScreen.GetComponent<LoadingScreen>();
        }

        private void OnTriggerEnter(Collider collider) {
            if (collider.gameObject.tag == "Player") {
                GameObject musicPlayer = GameObject.Find("DungeonMusic");
                Destroy(musicPlayer);
                StateManager.startNewDay();
                loadScript.startLoadingScreen("LoseGame");
            }
        }
    }
}
