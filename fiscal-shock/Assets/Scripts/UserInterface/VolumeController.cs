using UnityEngine;

public class VolumeController : MonoBehaviour {
    public AudioSource audioS { get; private set; }

    void Start() {
        audioS = GetComponent<AudioSource>();
        audioS.volume = Settings.volume;
    }
}
