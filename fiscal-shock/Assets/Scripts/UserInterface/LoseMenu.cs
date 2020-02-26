using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseMenu : MonoBehaviour
{

    public void Start(){
        Debug.Log(SceneManager.GetActiveScene().buildIndex);
        Debug.Log(SceneManager.GetActiveScene().name);
    }

    public void RetryClick (){
        SceneManager.LoadScene("Hub");
    }

    public void BankruptClick (){
        Debug.Log("Quit");
        Application.Quit();
    }

}
