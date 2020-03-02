using UnityEngine;

public class VolumeController : MonoBehaviour {
    public AudioSource audio { get; private set; }

    void Start() {
        audio = GetComponent<AudioSource>();
        audio.volume = Settings.volume;
    }
}
