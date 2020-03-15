using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class InGameMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject quitPanel;
    public GameObject player;
    public GameObject crossHair;
    public TextMeshProUGUI pauseText;
    private VolumeController[] volumeControllers;
    public Slider volumeSlider;
    public Slider mouseSlider;

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
        volumeSlider.onValueChanged.RemoveAllListeners();
        crossHair = GameObject.Find("Crosshair");
        volumeControllers = GameObject.FindObjectsOfType<VolumeController>();
        foreach (VolumeController vc in volumeControllers) {
            volumeSlider.onValueChanged.AddListener((value) => vc.GetComponent<AudioSource>().volume = value);
        }
        volumeSlider.onValueChanged.AddListener((value) => Settings.volume = value);
        mouseSlider.onValueChanged.AddListener((value) => Settings.mouseSensitivity = value);
        pausePanel.SetActive(false);
        quitPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    void Update() {
        // Bring up pause menu
        if (Input.GetKeyDown(Settings.pauseKey)) {
            if (!pausePanel.activeSelf) {
                Time.timeScale = 0;
                pauseText.text = "PAUSED";
                Settings.mutexUnlockCursorState(this);
                pausePanel.SetActive(true);
                crossHair?.SetActive(false);
            } else {
                optionsPanel.SetActive(false);
                quitPanel.SetActive(false);
                pausePanel.SetActive(false);
                crossHair?.SetActive(true);
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

    public void PlayClick ()
    {
        Settings.lockCursorState(this);
        pausePanel.SetActive(false);
        crossHair?.SetActive(true);
        Time.timeScale = 1;
        pauseText.text = "";
    }

    public void OptionsClick ()
    {
        optionsPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void QuitClick ()
    {
        quitPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void RestartClick ()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitAppClick ()
    {
        Application.Quit();
    }

    public void CancelClick ()
    {
        quitPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void BackClick()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}
