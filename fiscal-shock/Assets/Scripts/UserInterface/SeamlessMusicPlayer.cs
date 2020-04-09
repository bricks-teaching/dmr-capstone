using UnityEngine;

/// <summary>
/// Singleton that prevents things from being destroyed on scene changes.
/// Must be manually destroyed if it needs to go away.
/// </summary>
public class SeamlessMusicPlayer : MonoBehaviour {
     public static SeamlessMusicPlayer musicInstance { get; private set; }

     void Awake() {
         if (musicInstance != null && musicInstance != this) {
             Destroy(this.gameObject);
             return;
         } else {
             musicInstance = this;
         }
         DontDestroyOnLoad(this.gameObject);
         StateManager.singletons.Add(this.gameObject);
     }
}
