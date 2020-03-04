using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject quitPanel;
    public GameObject player;
    public GameObject crossHair;
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
            volumeSlider.onValueChanged.AddListener((value) => vc.audio.volume = value);
        }
        volumeSlider.onValueChanged.AddListener((value) => Settings.volume = value);
        mouseSlider.onValueChanged.AddListener((value) => Settings.mouseSensitivity = value);
        pausePanel.SetActive(false);
        quitPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    void Update() {
        // Bring up pause menu
        if (Input.GetKeyDown("p")) {
            if (!pausePanel.activeSelf) {
                if (Time.timeScale == 0) {
                    Time.timeScale = 1;
                } else {
                    Time.timeScale = 0;
                }
                Cursor.lockState = CursorLockMode.None;
                pausePanel.SetActive(true);
                crossHair?.SetActive(false);
            } else {
                optionsPanel.SetActive(false);
                quitPanel.SetActive(false);
                pausePanel.SetActive(false);
                crossHair?.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1;
            }
        }
    }

    public void PlayClick ()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pausePanel.SetActive(false);
        crossHair?.SetActive(true);
        Time.timeScale = 1;
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
