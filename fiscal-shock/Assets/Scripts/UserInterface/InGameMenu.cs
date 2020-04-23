using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InGameMenu : MonoBehaviour {
    public static InGameMenu inGameMenuInstance { get; private set; }
    public GameObject background;
    public GameObject pausePanel;
    public GameObject inventoryPanel;
    public GameObject optionsPanel;
    public GameObject quitPanel;
    public GameObject graphicsPanel;
    public GameObject creditBar;
    private List<GameObject> panels { get; } = new List<GameObject>();
    public GameObject player { get; set; }
    public TextMeshProUGUI pauseText;
    private VolumeController[] volumeControllers;
    public Slider volumeSlider;
    public Slider mouseSlider;

    [Header("Graphics Settings")]
    private TextMeshProUGUI fpsText;
    public GraphicsWidgets widgets;

    private bool loadedDropdowns;

    void Awake() {
        if (inGameMenuInstance != null && inGameMenuInstance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            inGameMenuInstance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        StateManager.singletons.Add(this.gameObject);
    }

    private void Start() {
        volumeSlider.value = Settings.volume;
        mouseSlider.value = Settings.mouseSensitivity;
        panels.Add(pausePanel);
        panels.Add(optionsPanel);
        panels.Add(quitPanel);
        panels.Add(graphicsPanel);
        panels.Add(inventoryPanel);
        panels.Add(creditBar);
        disableAllPanels();
        player = GameObject.FindGameObjectWithTag("Player");
        fpsText = GameObject.FindGameObjectWithTag("HUD").transform.Find("FPS").gameObject.GetComponent<TextMeshProUGUI>();
    }

    public float volume {
        get => Settings.volume;
        set => Settings.volume = value;
    }

    public float mouseSensitivity {
        get => Settings.mouseSensitivity;
        set => Settings.mouseSensitivity = value;
    }

    void OnEnable() {
        SceneManager.sceneLoaded += onSceneLoad;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= onSceneLoad;
    }

    /// <summary>
    /// Delegate to listen to scene load events. Needed to adjust volume
    /// controllers, which are attached to music players in the levels.
    /// Otherwise, the music players aren't affected by the settings volume.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void onSceneLoad(Scene scene, LoadSceneMode mode) {
        // Attach listeners for slider adjustments
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeControllers = GameObject.FindObjectsOfType<VolumeController>();
        foreach (VolumeController vc in volumeControllers) {
            volumeSlider.onValueChanged.AddListener((value) => vc.GetComponent<AudioSource>().volume = value);
        }
    }

    /// <summary>
    /// Disable all panels, as if you're going to unpause
    /// </summary>
    private void disableAllPanels() {
        foreach (GameObject p in panels) {
            p.SetActive(false);
        }
        background.SetActive(false);
    }

    /// <summary>
    /// Dismiss all panels except the one specified
    /// </summary>
    /// <param name="keep">leave this one open</param>
    private void disableAllPanelsExcept(GameObject keep) {
        foreach (GameObject p in panels) {
            p.SetActive(p == keep);
        }
    }

    /// <summary>
    /// Pause menu event handling.
    /// </summary>
    void Update() {
        if (StateManager.playerDead || StateManager.playerWon) {
            return;
        }
        // Was the game already paused or is the background image up, implying the game should be paused, but it's running and the menu is up?
        if (Input.GetKeyDown(Settings.pauseKey) && (Time.timeScale == 0 || background.activeSelf)) {
            disableAllPanels();
            pauseText.text = "";
            Time.timeScale = 1;
            Settings.forceLockCursorState();
            StateManager.pauseAvailable = true;
            return;
        }
        // Hide pause menus for screenshots
        if (Input.GetKeyDown(Settings.hidePauseMenuKey) && StateManager.pauseAvailable) {
            disableAllPanels();
            if (Time.timeScale > 0) {  // getting bugged in the pause menu lately
                background.SetActive(false);
                pauseText.text = "";
                Settings.mutexLockCursorState(this);
            }
            return;
        }
        // Bring up pause menu
        if (Input.GetKeyDown(Settings.pauseKey) && StateManager.pauseAvailable && Time.timeScale > 0) {
            System.GC.Collect();
            Time.timeScale = 0;
            pauseText.text = "PAUSED";
            Settings.forceUnlockCursorState();
            background.SetActive(true);
            creditBar.SetActive(true);
            pausePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Exits the pause menu and resumes gameplay.
    /// </summary>
    public void PlayClick() {
        Settings.lockCursorState(this);
        disableAllPanels();
        Time.timeScale = 1;
        pauseText.text = "";
    }

    public void OptionsClick() {
        disableAllPanelsExcept(optionsPanel);
    }

    public void QuitClick() {
        disableAllPanelsExcept(quitPanel);
    }

    public void inventoryClick() {
        disableAllPanelsExcept(inventoryPanel);
    }

    public void RestartClick() {
        pauseText.text = "";
        Settings.quitToMainMenu();
    }

    public void QuitAppClick() {
        pauseText.text = "";
        Settings.quitToDesktop();
    }

    public void CancelClick() {
        disableAllPanelsExcept(pausePanel);
        creditBar.SetActive(true);
    }

    public void BackClick() {
        disableAllPanelsExcept(pausePanel);
        creditBar.SetActive(true);
    }

    public void GraphicsClick() {
        disableAllPanelsExcept(graphicsPanel);
        if (!loadedDropdowns) {
            populateDropdowns();
            loadedDropdowns = true;
        }
    }

    /* graphics stuff */
    /// <summary>
    /// Update to a new quality preset
    /// </summary>
    /// <param name="level">Dropdown index, corresponds to a quality level in the array of valid presets</param>
    public void changeQualityLevel(int level) {
        Settings.values.currentQuality = level;
        Settings.values.currentQualityName = QualitySettings.names[level];
        Settings.qualityPreset = DefaultQualitySettings.getPresetByIndex(level);
        Settings.updateCurrentSettings();
        loadAllWidgetsFromCurrentState();
    }

    /// <summary>
    /// Reset all configurable settings to the defaults of the currently
    /// selected preset
    /// </summary>
    public void resetToDefaults() {
        Settings.resetToCurrentQualityDefaults();
        loadAllWidgetsFromCurrentState();
        widgets.overrideToggle.interactable = false;
        widgets.overrideToggle.isOn = false;
        widgets.overrideToggle.interactable = true;
    }

    /// <summary>
    /// Toggles whether to apply configurable graphics settings or use the
    /// quality preset defaults.
    /// </summary>
    /// <param name="toggle"></param>
    public void toggleQualityOverrides(bool toggle) {
        // interactable check is a hack, because every time the value of toggle.isOn is changed, this function is called
        if (widgets.overrideToggle.interactable) {
            Settings.values.overrideQualitySettings = toggle;
            Settings.updateCurrentSettings();
            loadAllWidgetsFromCurrentState();
        }
    }

    /// <summary>
    /// Change vsync count for software vertical sync. A vsyncCount
    /// of 0 is required to cap the frame rate, otherwise, the
    /// monitor's refresh rate is the target framerate.
    /// </summary>
    /// <param name="v"></param>
    public void setVsyncCount(int v) {
        changedAnySetting();
        Settings.values.vsyncCount = v;
        QualitySettings.vSyncCount = Settings.values.vsyncCount;
    }

    /// <summary>
    /// Updates texture anti-aliasing settings. Values should be
    /// powers of: 0, 2, 4, 8 are the only sampling resolutions
    /// available in Unity.
    /// </summary>
    /// <param name="aa"></param>
    public void setAntialiasing(int aa) {
        changedAnySetting();
        // indices are powers of 2 for aa samples
        Settings.values.antialiasingSamples = (aa == 0? aa : 1 << aa);
        QualitySettings.antiAliasing = Settings.values.antialiasingSamples;
    }

    /// <summary>
    /// Updates number of times light is calculated on a pixel. More lights
    /// means better quality lighting, but drastically reduces performance.
    /// </summary>
    /// <param name="lights">number of lights per pixel</param>
    public void setPixelLighting(int lights) {
        changedAnySetting();
        Settings.values.pixelLightCount = lights;
        QualitySettings.pixelLightCount = Settings.values.pixelLightCount;
    }

    /// <summary>
    /// Updates shadow resolution settings. Hard-coded to use
    /// hard shadows except on the highest resolutions. Soft
    /// shadows are more graphics-intensive.
    /// </summary>
    /// <param name="res"></param>
    public void setShadowRes(int res) {
        changedAnySetting();
        if (res == 0) {
            QualitySettings.shadows = ShadowQuality.Disable;
        } else if (res < 3) {
            QualitySettings.shadows = ShadowQuality.HardOnly;
        } else {
            QualitySettings.shadows = ShadowQuality.All;
        }
        Settings.values.shadowResolution = (ShadowResolution)res;
        QualitySettings.shadowResolution = Settings.values.shadowResolution;
    }

    /// <summary>
    /// Sets the distance at which the camera starts rendering shadows.
    /// Should never be higher than the far clip plane of the camera (128).
    /// Setting this high is bad news! In dungeons, the player probably
    /// can't see shadows farther than ~50 units due to perspective.
    /// </summary>
    /// <param name="dist"></param>
    public void setShadowDistance(float dist) {
        changedAnySetting();
        Settings.values.shadowDistance = (int)dist;
        QualitySettings.shadowDistance = Settings.values.shadowDistance;
    }

    /// <summary>
    /// Sets anisotropic filtering quality. Anisotropic filtering smooths
    /// out textures that are "stretched." Most noticeable on walls that
    /// get squished textures sometimes.
    /// </summary>
    /// <param name="ani"></param>
    public void setAnisotropic(int ani) {
        changedAnySetting();
        Settings.values.anisotropicTextures = (AnisotropicFiltering)ani;
        QualitySettings.anisotropicFiltering = Settings.values.anisotropicTextures;
    }

    /// <summary>
    /// Not in the options menu. Requires vsyncCount to be 0 to be
    /// applied anyway.
    /// </summary>
    /// <param name="fps"></param>
    public void setFramerate(int fps) {
        changedAnySetting();
        Settings.values.targetFramerate = fps;
        Application.targetFrameRate = Settings.values.targetFramerate;
    }

    /// <summary>
    /// If any setting is customized, we should tick the "override" box,
    /// because that's implied by changing a setting.
    /// </summary>
    public void changedAnySetting() {
        widgets.overrideToggle.interactable = false;
        widgets.overrideToggle.isOn = true;
        widgets.overrideToggle.interactable = true;
        Settings.values.overrideQualitySettings = true;
        widgets.qualityText.text = $"Current Quality: {Settings.values.currentQualityName}{(Settings.values.overrideQualitySettings? "*" : "")}";
    }

    /// <summary>
    /// Toggles display of FPS in the top right of the screen.
    /// </summary>
    /// <param name="toggle"></param>
    public void toggleFPS(bool toggle) {
        try {  // fpsText isn't always instantiated in time
            Settings.values.showFPS = toggle;
            fpsText.enabled = toggle;
        } catch {}
    }

    /// <summary>
    /// Toggles windowed or full screen mode.
    /// </summary>
    /// <param name="toggle"></param>
    public void toggleFullscreen(bool toggle) {
        Settings.values.fullscreen = toggle;
    }

    /// <summary>
    /// Updates the game window resolution. Strange things will
    /// happen if the player changes their valid resolutions
    /// during play (the dropdown is never updated). This wouldn't
    /// happen unless the player hotplugged a different monitor or
    /// graphics card, though. Rare cases.
    /// </summary>
    /// <param name="i"></param>
    public void setResolution(int i) {
        Resolution r = Settings.getResolutionByIndex(i);
        Settings.values.resolutionWidth = r.width;
        Settings.values.resolutionHeight = r.height;
    }

    /// <summary>
    /// Sets up all the graphics widgets to the proper value
    /// based on what's in the settings file.
    /// </summary>
    public void loadAllWidgetsFromCurrentState() {
        widgets.qualityText.text = $"Current Quality: {Settings.values.currentQualityName}{(Settings.values.overrideQualitySettings? "*" : "")}";
        widgets.qualityDropdown.value = Settings.values.currentQuality;
        widgets.showFPSToggle.isOn = Settings.values.showFPS;
        widgets.antialiasing.value = (int)System.Math.Log(Settings.values.antialiasingSamples, 2);
        widgets.vsyncDropdown.value = Settings.values.vsyncCount;
        widgets.pixelLighting.value = Settings.values.pixelLightCount;
        widgets.shadowRes.value = (int)Settings.values.shadowResolution;
        widgets.shadowDistance.value = Settings.values.shadowDistance;
        widgets.anisotropic.value = (int)Settings.values.anisotropicTextures;
        widgets.overrideToggle.interactable = false;
        widgets.overrideToggle.isOn = Settings.values.overrideQualitySettings;
        widgets.overrideToggle.interactable = true;
        widgets.fullscreenToggle.interactable = false;
        widgets.fullscreenToggle.isOn = Settings.values.fullscreen;
        widgets.fullscreenToggle.interactable = true;
    }

    /// <summary>
    /// Populates drop downs. Should only ever happen once!
    /// </summary>
    public void populateDropdowns() {
        widgets.qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        widgets.antialiasing.AddOptions(new List<string>(Settings.antialiasingNames));
        widgets.vsyncDropdown.AddOptions(new List<string>(Settings.vsyncCountNames));
        widgets.pixelLighting.AddOptions(new List<string>(Settings.pixelQualityNames));
        widgets.shadowRes.AddOptions(new List<string>(Settings.shadowResNames));
        widgets.anisotropic.AddOptions(new List<string>(Settings.anisotropicNames));
        widgets.resolution.AddOptions(Settings.getSupportedResolutions());
    }

    /// <summary>
    /// Close the graphics panel. Implies going back to the
    /// main options panel.
    /// </summary>
    public void closeGraphics() {
        disableAllPanelsExcept(optionsPanel);
        Settings.updateCurrentSettings();
    }
}

/// <summary>
/// Data class to hold a bunch of stuff in inspector view
/// without clogging up the script view.
/// </summary>
[System.Serializable]
public class GraphicsWidgets {
    public TextMeshProUGUI qualityText;
    public Toggle overrideToggle;
    public Toggle showFPSToggle;
    public Toggle fullscreenToggle;
    public Slider shadowDistance;

    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown vsyncDropdown;
    public TMP_Dropdown pixelLighting;
    public TMP_Dropdown shadowRes;
    public TMP_Dropdown anisotropic;
    public TMP_Dropdown antialiasing;
    public TMP_Dropdown resolution;
}
