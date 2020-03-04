using UnityEngine;

/// <summary>
/// Singleton that prevents things from being destroyed on scene changes.
/// Must be manually destroyed if it needs to go away.
/// </summary>
public class MenuSingleton : MonoBehaviour {
     public static MenuSingleton menuInstance { get; private set; }

     void Awake() {
         if (menuInstance != null && menuInstance != this) {
             Destroy(this.gameObject);
             return;
         } else {
             menuInstance = this;
         }
         DontDestroyOnLoad(this.gameObject);
     }
}
