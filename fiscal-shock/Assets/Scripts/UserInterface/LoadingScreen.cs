using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

/// <summary>
/// Based on https://gist.github.com/nickpettit/a78cc0a9483c85212a23
/// </summary>
public class LoadingScreen : MonoBehaviour {
    string nextScene = "Hub";

    private TextMeshProUGUI loadingText;
    public Slider progressBar;
    public static LoadingScreen loadScreenInstance { get; private set; }
    private AsyncOperation async;
    public Camera loadCamera;
    public Color loadingColor;
    public Color doneColor;
    public Image progressFill;
    public TextMeshProUGUI percentText;

    private readonly string templeStory = "Hostile robots are excavating the Ruins of Tehamahouti, stealing every shiny object they can get their hands on. Clear out the robots before it becomes a total archaeological loss! Oh, and try not to die.\n\n";

    private readonly string mineStory = "We have traced a cache of black market gold and gemstones to a series of mines. Naturally, BOTCORP is the culprit. Due to the CEO's affiliation and close ties with illegal markets, we believe that he is storing stolen artifacts for resale here, as well. Your job is the same as always: crush the bots. Our specialists will come in and take care of the rest.\n\n";

    private bool clickTextAdded;
    private string defaultText = "Loading...\n\n";

    void Awake() {
        if (loadScreenInstance != null && loadScreenInstance != this) {
            Destroy(this.gameObject);
            return;
        }
        else {
            loadScreenInstance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Start() {
        loadingText = GameObject.Find("LoadText").GetComponent<TextMeshProUGUI>();
        loadCamera.enabled = false;
        loadingText.text = defaultText;
    }

    void Update() {
        // If the new scene has started loading...
        if (async != null) {
            // ...then pulse the transparency of the loading text to let the player know that the computer is still working.
            progressBar.value = Mathf.Clamp01(async.progress / 0.9f);
            if (progressBar.value > 0.9f && !clickTextAdded) {
                progressFill.color = doneColor;
                loadingText.text += "<i>Press the Left Mouse Button to continue.</i>";
                clickTextAdded = true;
            }
            percentText.text = $"{progressBar.value * 100}%";
            if (Input.GetMouseButtonDown(0)) {
                async.allowSceneActivation = true;
            }
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
        nextScene = sceneToLoad;
        StartCoroutine(loadScene());
    }

    private IEnumerator<WaitForSeconds> loadScene() {
        loadCamera.enabled = true;
        progressBar.value = 0;
        progressFill.color = loadingColor;
        async = SceneManager.LoadSceneAsync(nextScene);
        switch (StateManager.selectedDungeon) {
            case DungeonTypeEnum.Temple:
                async.allowSceneActivation = false;
                loadingText.text = templeStory;
                break;
            case DungeonTypeEnum.Mine:
                async.allowSceneActivation = false;
                loadingText.text = mineStory;
                break;
            default:
                loadingText.text = defaultText;
                break;
        }

        while (!async.isDone) {
            yield return null;
        }

        async = null;
        loadCamera.enabled = false;
        loadingText.text = defaultText;
        clickTextAdded = false;
    }
}
