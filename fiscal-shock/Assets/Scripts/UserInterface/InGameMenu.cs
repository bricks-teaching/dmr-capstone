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
    public GameObject optionsPanel;
    public GameObject quitPanel;
    public GameObject graphicsPanel;
    private List<GameObject> panels { get; } = new List<GameObject>();
    public GameObject player { get; set; }
    public TextMeshProUGUI pauseText;
    private VolumeController[] volumeControllers;
    public Slider volumeSlider;
    public Slider mouseSlider;

    [Header("Graphics Settings")]
    private TextMeshProUGUI fpsText;
    public GraphicsWidgets widgets;

    void Awake() {
        if (inGameMenuInstance != null && inGameMenuInstance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            inGameMenuInstance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        StateManager.singletons.Add(this.gameObject);
        populateDropdowns();
    }

    private void Start() {
        volumeSlider.value = Settings.volume;
        mouseSlider.value = Settings.mouseSensitivity;
        panels.Add(pausePanel);
        panels.Add(optionsPanel);
        panels.Add(quitPanel);
        panels.Add(graphicsPanel);
        disableAllPanels();
        player = GameObject.FindGameObjectWithTag("Player");
        fpsText = GameObject.FindGameObjectWithTag("HUD").transform.Find("FPS").gameObject.GetComponent<TextMeshProUGUI>();
        loadAllWidgetsFromCurrentState();
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

    void onSceneLoad(Scene scene, LoadSceneMode mode) {
        // Attach listeners for slider adjustments
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeControllers = GameObject.FindObjectsOfType<VolumeController>();
        foreach (VolumeController vc in volumeControllers) {
            volumeSlider.onValueChanged.AddListener((value) => vc.GetComponent<AudioSource>().volume = value);
        }
    }

    private void disableAllPanels() {
        foreach (GameObject p in panels) {
            p.SetActive(false);
        }
        background.SetActive(false);
    }

    private void disableAllPanelsExcept(GameObject keep) {
        foreach (GameObject p in panels) {
            p.SetActive(p == keep);
        }
    }

    void Update() {
        // Bring up pause menu
        if (Input.GetKeyDown(Settings.pauseKey) && StateManager.pauseAvailable) {
            if (Time.timeScale > 0) {
                System.GC.Collect();
                Time.timeScale = 0;
                pauseText.text = "PAUSED";
                Settings.forceUnlockCursorState();
                background.SetActive(true);
                pausePanel.SetActive(true);
            } else {
                disableAllPanels();
                Settings.forceLockCursorState();
                Time.timeScale = 1;
                pauseText.text = "";
            }
        }
        if (Input.GetKeyDown(Settings.hidePauseMenuKey) && Time.timeScale == 0) {
                disableAllPanels();
                // no longer a toggle
        }
    }

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
    }

    public void BackClick() {
        disableAllPanelsExcept(pausePanel);
    }

    public void GraphicsClick() {
        disableAllPanelsExcept(graphicsPanel);
    }

    /* graphics stuff */
    public void changeQualityLevel(int level) {
        Settings.values.currentQuality = level;
        Settings.values.currentQualityName = QualitySettings.names[level];
        Settings.qualityPreset = DefaultQualitySettings.getPresetByIndex(level);
        Settings.updateCurrentSettings();
        loadAllWidgetsFromCurrentState();
    }

    public void resetToDefaults() {
        Settings.resetToCurrentQualityDefaults();
        loadAllWidgetsFromCurrentState();
        widgets.overrideToggle.interactable = false;
        widgets.overrideToggle.isOn = false;
        widgets.overrideToggle.interactable = true;
    }

    public void toggleQualityOverrides(bool toggle) {
        // interactable check is a hack, because every time the value of toggle.isOn is changed, this function is called
        if (widgets.overrideToggle.interactable) {
            Settings.values.overrideQualitySettings = toggle;
            Settings.updateCurrentSettings();
            loadAllWidgetsFromCurrentState();
        }
    }

    public void setVsyncCount(int v) {
        changedAnySetting();
        Settings.values.vsyncCount = v;
        QualitySettings.vSyncCount = Settings.values.vsyncCount;
    }

    public void setAntialiasing(int aa) {
        changedAnySetting();
        // indices are powers of 2 for aa samples
        Settings.values.antialiasingSamples = (aa == 0? aa : 1 << aa);
        QualitySettings.antiAliasing = Settings.values.antialiasingSamples;
    }

    public void setPixelLighting(int lights) {
        changedAnySetting();
        Settings.values.pixelLightCount = lights;
        QualitySettings.pixelLightCount = Settings.values.pixelLightCount;
    }

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

    public void setShadowDistance(float dist) {
        changedAnySetting();
        Settings.values.shadowDistance = (int)dist;
        QualitySettings.shadowDistance = Settings.values.shadowDistance;
    }

    public void setAnisotropic(int ani) {
        changedAnySetting();
        Settings.values.anisotropicTextures = (AnisotropicFiltering)ani;
        QualitySettings.anisotropicFiltering = Settings.values.anisotropicTextures;
    }

    public void setFramerate(int fps) {
        changedAnySetting();
        Settings.values.targetFramerate = fps;
        Application.targetFrameRate = Settings.values.targetFramerate;
    }

    public void changedAnySetting() {
        widgets.overrideToggle.interactable = false;
        widgets.overrideToggle.isOn = true;
        widgets.overrideToggle.interactable = true;
        Settings.values.overrideQualitySettings = true;
        widgets.qualityText.text = $"Current Quality: {Settings.values.currentQualityName}{(Settings.values.overrideQualitySettings? "*" : "")}";
    }

    public void toggleFPS(bool toggle) {
        try {  // fpsText isn't always instantiated in time
            Settings.values.showFPS = toggle;
            fpsText.enabled = toggle;
        } catch {}
    }

    public void toggleFullscreen(bool toggle) {
        Settings.values.fullscreen = toggle;
    }

    public void setResolution(int i) {
        Resolution r = Settings.getResolutionByIndex(i);
        Settings.values.resolutionWidth = r.width;
        Settings.values.resolutionHeight = r.height;
    }

    /// <summary>
    /// sets up all the graphics widgets to the proper value
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

    public void populateDropdowns() {
        widgets.qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        widgets.antialiasing.AddOptions(new List<string>(Settings.antialiasingNames));
        widgets.vsyncDropdown.AddOptions(new List<string>(Settings.vsyncCountNames));
        widgets.pixelLighting.AddOptions(new List<string>(Settings.pixelQualityNames));
        widgets.shadowRes.AddOptions(new List<string>(Settings.shadowResNames));
        widgets.anisotropic.AddOptions(new List<string>(Settings.anisotropicNames));
        widgets.resolution.AddOptions(Settings.getSupportedResolutions());
    }

    public void closeGraphics() {
        disableAllPanelsExcept(optionsPanel);
        Settings.updateCurrentSettings();
    }
}

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
