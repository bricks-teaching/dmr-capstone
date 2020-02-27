using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinSceen : MonoBehaviour
{
    public void QuitClick (){
        Debug.Log("Quit");
        Application.Quit();
    }
}
