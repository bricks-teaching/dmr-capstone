using UnityEngine;

/// <summary>
/// Singleton that prevents things from being destroyed on scene changes.
/// Must be manually destroyed if it needs to go away.
/// </summary>
public class HUDSingleton : MonoBehaviour {
     public static HUDSingleton hudInstance { get; private set; }

     void Awake() {
         if (hudInstance != null && hudInstance != this) {
             Destroy(this.gameObject);
             return;
         } else {
             hudInstance = this;
         }
         DontDestroyOnLoad(this.gameObject);
         StateManager.singletons.Add(this.gameObject);
     }
}
