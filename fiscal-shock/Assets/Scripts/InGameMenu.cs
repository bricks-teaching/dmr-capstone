using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject quitPanel;
    public GameObject controller;
    public GameObject player;
    public GameObject crossHair;
    void Start()
    {
        pausePanel.SetActive(false);
        quitPanel.SetActive(false);
        optionsPanel.SetActive(false);
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
        WeaponDemo demoScript = controller.GetComponent(typeof(WeaponDemo)) as WeaponDemo;
        demoScript.ChangeVolume(value);
    }
    public void AdjustMouseSensitivity(float value)
    {
        Debug.Log("Sensitivity is - " + value);
        MouseLook cameraScript = player.GetComponent(typeof(MouseLook)) as MouseLook;
        cameraScript.mouseSensitivity = value;
    }
    public void BackClick ()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }


}
