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

    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI clickText;
    public Slider progressBar;
    public static LoadingScreen loadScreenInstance { get; private set; }
    private AsyncOperation async;
    public Camera loadCamera;
    public Color loadingColor;
    public Color doneColor;
    public Image progressFill;
    public TextMeshProUGUI percentText;
    public string previousScene { get; private set; }
    public GameObject tombstone;

    private readonly string templeStory = "Hostile robots are excavating the Ruins of Tehamahouti, stealing every shiny object they can get their hands on. Clear out the robots before it becomes a total archaeological loss! Oh, and try not to die.";

    private readonly string mineStory = "We have traced a cache of black market gold and gemstones to a series of mines. Naturally, BOTCORP is the culprit. Due to the CEO's affiliation and close ties with illegal markets, we believe that he is storing stolen artifacts for resale here, as well. Your job is the same as always: crush the bots. Our specialists will come in and take care of the rest.";

    private readonly string defaultText = "Loading...";

    private readonly string[] eulogies = {
        "Suffocated under a pile of loans.",
        "peperony and chease",
        "Couldn't pay loans with charm and good looks.",
        "Can't pay off a payday loan if you don't get paid.",
        "Can't pay off a payday loan if you don't survive 'til payday.",
        "Bury me with my money!",
        "YOU DIED",
        "Their financial plan didn't make much cents.",
        "Dead broke"
    };

    void Awake() {
        if (loadScreenInstance != null && loadScreenInstance != this) {
            Destroy(this.gameObject);
            return;
        }
        else {
            loadScreenInstance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        // Do not add to StateManager.singletons unless you never want to use the load screen to get to the menu
    }

    void Start() {
        loadCamera.enabled = false;
        loadingText.text = defaultText;
        clickText.enabled = false;
    }

    void Update() {
        // If the new scene has started loading...
        if (async != null) {
            // ...then pulse the transparency of the loading text to let the player know that the computer is still working.
            progressBar.value = Mathf.Clamp01(async.progress / 0.9f);
            if (progressBar.value > 0.9f && !clickText.enabled) {
                progressFill.color = doneColor;
                clickText.enabled = true;
            }
            percentText.text = $"{(int)(progressBar.value * 100)}%";
            if (Input.GetMouseButtonDown(0) && (async.progress > 0.8f || nextScene != "Dungeon")) {
                async.allowSceneActivation = true;
                StartCoroutine(restartTime());
                clickText.text = "Please wait...";
            }
        }
    }

    private IEnumerator restartTime() {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1;
        yield return null;
    }

    /// <summary>
    /// Disables any scripts passed in and then starts the coroutine to begin
    /// asynchronous loading of the next scene while keeping a loading screen
    /// on. Always disable the player scripts and enemy scripts.
    /// </summary>
    /// <param name="sceneToLoad"></param>
    public void startLoadingScreen(string sceneToLoad) {
        StateManager.pauseAvailable = false;
        System.GC.Collect();
        previousScene = SceneManager.GetActiveScene().name;
        nextScene = sceneToLoad;
        StartCoroutine(loadScene());
    }

    private IEnumerator<WaitForSeconds> loadScene() {
        tombstone.SetActive(false);
        loadCamera.enabled = true;
        progressBar.value = 0;
        progressFill.color = loadingColor;
        Time.timeScale = 0;
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
        if (StateManager.playerDead) {
            loadingText.text = "";
            tombstone.GetComponentInChildren<TextMeshProUGUI>().text = $"{eulogies[Random.Range(0, eulogies.Length)]}";
            tombstone.SetActive(true);
        }

        while (!async.isDone) {
            yield return null;
        }

        async = null;
        loadCamera.enabled = false;
        StateManager.pauseAvailable = true;
        loadingText.text = defaultText;
        clickText.enabled = false;
        clickText.text = "Press the Left Mouse Button to continue.";
    }
}
