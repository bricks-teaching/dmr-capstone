using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script is to allow main camera movement in the Weapons Demo scene. It can be deleted once this demo scene is deleted.
public class WeaponDemo : MonoBehaviour
{
    public GameObject enemy1;
    public GameObject enemy2;
    public float spawnRate = 10.0f;
    private float time = 9.0f;

    // Update is called once per frame
    public void Update()
    { 
        //Spawn a new bot when time passed reaches spawnRate
        time += Time.deltaTime;
        if(time > spawnRate){
            time = 0.0f;
            if(Random.value > 0.5){
                GameController.spawnBot(enemy2, false);
            } else {
                GameController.spawnBot(enemy1, true);
            }
        }
    }

    
}
