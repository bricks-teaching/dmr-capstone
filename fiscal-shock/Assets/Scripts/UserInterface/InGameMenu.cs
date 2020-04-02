using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class InGameMenu : MonoBehaviour {
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject quitPanel;
    public GameObject player { get; set; }
    public TextMeshProUGUI pauseText;
    private VolumeController[] volumeControllers;
    public Slider volumeSlider;
    public Slider mouseSlider;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
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
        // Match up sliders to actual values
        volumeSlider.value = Settings.volume;
        mouseSlider.value = Settings.mouseSensitivity;

        // Attach listeners for slider adjustments
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeControllers = GameObject.FindObjectsOfType<VolumeController>();
        foreach (VolumeController vc in volumeControllers) {
            volumeSlider.onValueChanged.AddListener((value) => vc.GetComponent<AudioSource>().volume = value);
        }
        volumeSlider.onValueChanged.AddListener((value) => Settings.volume = value);
        mouseSlider.onValueChanged.AddListener((value) => Settings.mouseSensitivity = value);

        // Disable the panels
        pausePanel.SetActive(false);
        quitPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    void Update() {
        // Bring up pause menu
        if (Input.GetKeyDown(Settings.pauseKey)) {
            if (!pausePanel.activeSelf) {
                System.GC.Collect();
                Time.timeScale = 0;
                pauseText.text = "PAUSED";
                Settings.mutexUnlockCursorState(this);
                pausePanel.SetActive(true);
            }
            else {
                optionsPanel.SetActive(false);
                quitPanel.SetActive(false);
                pausePanel.SetActive(false);
                Settings.lockCursorState(this);
                Time.timeScale = 1;
                pauseText.text = "";
            }
        }
        if (Input.GetKeyDown(Settings.hidePauseMenuKey)) {
            if (Time.timeScale == 0) {
                pausePanel.SetActive(!pausePanel.activeSelf);
            }
        }
    }

    public void PlayClick() {
        Settings.lockCursorState(this);
        pausePanel.SetActive(false);
        Time.timeScale = 1;
        pauseText.text = "";
    }

    public void OptionsClick() {
        optionsPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void QuitClick() {
        quitPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void RestartClick() {
        SceneManager.LoadScene(0);
    }

    public void QuitAppClick() {
        Settings.saveSettings();
        Application.Quit();
    }

    public void CancelClick() {
        quitPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void BackClick() {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void graphic(int i) {
        QualitySettings.SetQualityLevel(i, true);
    }
}
