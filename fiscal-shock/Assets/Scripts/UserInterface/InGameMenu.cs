using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject quitPanel;
    public GameObject player;
    public GameObject crossHair;
    private VolumeController[] volumeControllers;

    void Start()
    {
        crossHair = GameObject.Find("Crosshair");
        volumeControllers = GameObject.FindObjectsOfType<VolumeController>();
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
                crossHair.SetActive(false);
            } else {
                optionsPanel.SetActive(false);
                quitPanel.SetActive(false);
                pausePanel.SetActive(false);
                crossHair.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1;
            }
        }
    }

    public void PlayClick ()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pausePanel.SetActive(false);
        crossHair.SetActive(true);
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

    public void AdjustSFX(float value)
    {
        Debug.Log("Volume is - " + value);
        Settings.volume = value;
        foreach (VolumeController vc in volumeControllers) {
            vc.audio.volume = Settings.volume;
        }
    }

    public void AdjustMouseSensitivity(float value)
    {
        Debug.Log("Sensitivity is - " + value);
        Settings.mouseSensitivity = value;
    }

    public void BackClick()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}
