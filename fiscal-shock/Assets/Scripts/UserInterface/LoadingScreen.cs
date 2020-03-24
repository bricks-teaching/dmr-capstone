using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

/// <summary>
/// Based on https://gist.github.com/nickpettit/a78cc0a9483c85212a23
/// </summary>
public class LoadingScreen : MonoBehaviour {
    private bool currentlyLoading;
    string nextScene = "Hub";

    private TextMeshProUGUI loadingText;

    void Start() {
        loadingText = GameObject.Find("LoadText").GetComponent<TextMeshProUGUI>();
        GameObject.Find("LoadCamera").GetComponent<Camera>().enabled = false;
    }

    void Update() {
        // If the new scene has started loading...
        if (currentlyLoading) {
            // ...then pulse the transparency of the loading text to let the player know that the computer is still working.
            loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));
        }
    }

    /// <summary>
    /// Disables any scripts passed in and then starts the coroutine to begin
    /// asynchronous loading of the next scene while keeping a loading screen
    /// on. Always disable the player scripts and enemy scripts.
    /// </summary>
    /// <param name="sceneToLoad"></param>
    public void startLoadingScreen(string sceneToLoad) {
        System.GC.Collect();
        currentlyLoading = true;
        nextScene = sceneToLoad;
        Time.timeScale = 0;
        GameObject.Find("LoadCamera").GetComponent<Camera>().enabled = true;
        StartCoroutine(loadScene());
        Time.timeScale = 1;
    }

    private IEnumerator<WaitForSeconds> loadScene() {
        // Uncomment line to see loading screen if you're not on a potato
        //yield return new WaitForSeconds(3);

        AsyncOperation async = SceneManager.LoadSceneAsync(nextScene);

        while (!async.isDone) {
            yield return null;
        }
    }
}
