using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject quitPanel;
    public GameObject player;
    public GameObject crossHair;
    public static float volume = .3f;

    void Start()
    {
        crossHair = GameObject.Find("Crosshair");
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
                pausePanel.SetActive(false);
                optionsPanel.SetActive(false);
                quitPanel.SetActive(false);
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
        volume = value;
        ChangeVolume();
    }

    public void AdjustMouseSensitivity(float value)
    {
        Debug.Log("Sensitivity is - " + value);
        MouseLook cameraScript = player.GetComponent(typeof(MouseLook)) as MouseLook;
        cameraScript.mouseSensitivity = value;
    }

    public void BackClick()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void ChangeVolume() {
        // Go through all the scripts attached to player and the bots in the scene and update the volume
        player.GetComponentInChildren<PlayerShoot>().volume = volume;
        // No, this can't be function'd out easily because types :)
        // Have fun figuring out reflection if you want to play that game.
        foreach (EnemyShoot script in GameObject.FindObjectsOfType<EnemyShoot>()) {
            script.volume = volume;
        }
        foreach (EnemyHealth script in GameObject.FindObjectsOfType<EnemyHealth>()) {
            script.volume = volume;
        }
    }
}
