using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPoint : MonoBehaviour {
    public bool autoSpawn = true;
    public GameObject playerPrefab;
    private static SpawnPoint spawnPointInstance;
    private Vector3 defaultHubPos = new Vector3(3.117362f, 1.2f, -7.210602f);
    private Quaternion defaultHubRotation = Quaternion.Euler(0, 90, 0);

    void Awake() {
        if (spawnPointInstance != null && spawnPointInstance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            spawnPointInstance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        StateManager.singletons.Add(gameObject);
    }

    void OnEnable() {
        SceneManager.sceneLoaded += onSceneLoad;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= onSceneLoad;
    }

    void onSceneLoad(Scene s, LoadSceneMode ss) {
        if (autoSpawn) {
            spawnPlayer();
            Settings.forceLockCursorState();
        }
    }

    void Start() {
        onSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    public void resetToHubDefaults() {
        transform.position = defaultHubPos;
        transform.rotation = defaultHubRotation;
        autoSpawn = true;
        GameObject.FindGameObjectWithTag("Player Flashlight").GetComponent<Light>().intensity = 0;
        PlayerShoot shootScript = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerShoot>();
        shootScript.weapon?.SetActive(false);
        shootScript.crossHair.enabled = false;
        shootScript.enabled = false;
    }

    public GameObject spawnNewPlayer() {
        if (transform.position == Vector3.zero) {
            Debug.LogError($"No spawn point was set! Defaulting to {transform.position}");
        }
        return Instantiate(playerPrefab, transform.position, transform.rotation);
    }

    public GameObject spawnPlayer() {
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer == null) {
            return spawnNewPlayer();
        }
        if (transform.position == Vector3.zero) {
            Debug.LogError($"No spawn point was set! Defaulting to {transform.position}");
        }
        existingPlayer.GetComponentInChildren<PlayerMovement>().teleport(transform.position);
        existingPlayer.transform.rotation = transform.rotation;
        Debug.Log($"Spawned player at {transform.position}");

        return existingPlayer;
    }
}
