using System.Collections.Generic;
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

    public GameObject tutorial;
    private bool story = true;
    private int showing = 0;
    private int storyPosition = 0;
    private string[] stories0 = {"Hostile robots are excavating the Ruins of Tehamahouti,",
                                "stealing every shiny object they can get their hands on.",
                                "Clear out the robots before it becomes a total archaeological loss!",
                                "Oh, and try not to die."};
                    
    private string[] stories1 = {"We have traced a cache of black market gold and gemstones to a series of mines around the world.",
                                "Naturally, BOTCORP is the culprit. Due to his affiliation and close ties with illegal markets,",
                                "we believe that he is storing stolen artifacts for resale here as well.",
                                "Your job is the same as always, crash the bots.",
                                "Our specialists will come in and take care of the rest."};

    private string[] stories;

    void Awake() {
        if (loadScreenInstance != null && loadScreenInstance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            loadScreenInstance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Start() {
        loadingText = GameObject.Find("LoadText").GetComponent<TextMeshProUGUI>();
        loadCamera.enabled = false;
    }

    void Update() {
        // If the new scene has started loading...
        if (async != null) {
            // ...then pulse the transparency of the loading text to let the player know that the computer is still working.
            if(!story){
                loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));
            }
            // Unity loads to 90% and then activates stuff, so it gets "stuck" at 90%, nothing to do about it
            progressBar.value = Mathf.Clamp01(async.progress / 0.9f);
            if (progressBar.value > 0.9f) {
                progressFill.color = doneColor;
            }
            percentText.text = $"{progressBar.value * 100}%";
            if(Input.GetMouseButtonDown(0)){
                storyPosition++;
                if(storyPosition < stories.Length){
                    loadingText.text = stories[storyPosition];
                } else {
                    loadingText.text = "loading...";
                    async.allowSceneActivation = true;
                    story = false;
                }
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
        if(story){
            async.allowSceneActivation = false;
        }
        if(StateManager.selectedDungeon == (DungeonTypeEnum)0){
            stories = stories0;
        } else {
            stories = stories1;
        }

        loadingText.text = stories[0];
        while (!async.isDone) {
            yield return null;
        }

        async = null;
        loadCamera.enabled = false;
    }
}
