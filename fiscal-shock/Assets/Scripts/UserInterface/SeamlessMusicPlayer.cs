using UnityEngine;

/// <summary>
/// Singleton that prevents this from being destroyed on load, so
/// music plays seamlessly throughout the dungeon.
/// Must be manually destroyed when the music should change!
/// </summary>
public class SeamlessMusicPlayer : MonoBehaviour {
     public static SeamlessMusicPlayer instance { get; private set; }

     void Awake() {
         if (instance != null && instance != this) {
             Destroy(this.gameObject);
             return;
         } else {
             instance = this;
         }
         DontDestroyOnLoad(this.gameObject);
     }
}
