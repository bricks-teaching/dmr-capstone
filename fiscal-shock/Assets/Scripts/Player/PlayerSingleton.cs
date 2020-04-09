using UnityEngine;

/// <summary>
/// Singleton that prevents things from being destroyed on scene changes.
/// Must be manually destroyed if it needs to go away.
/// </summary>
public class PlayerSingleton : MonoBehaviour {
     public static PlayerSingleton playerInstance { get; private set; }

     void Awake() {
         if (playerInstance != null && playerInstance != this) {
             Destroy(this.gameObject);
             return;
         } else {
             playerInstance = this;
         }
         DontDestroyOnLoad(this.gameObject);
         StateManager.singletons.Add(this.gameObject);
     }
}
