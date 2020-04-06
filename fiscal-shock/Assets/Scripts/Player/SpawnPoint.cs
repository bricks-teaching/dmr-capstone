using UnityEngine;

public class SpawnPoint : MonoBehaviour {
    public bool autoSpawn = true;
    public GameObject playerPrefab;
    private static SpawnPoint spawnPointInstance;

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

    void Start() {
        if (autoSpawn) {
            spawnPlayer();
        }
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
