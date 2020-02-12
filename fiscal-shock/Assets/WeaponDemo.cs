using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is to allow main camera movement in the Weapons Demo scene. It can be deleted once this demo scene is deleted.
public class WeaponDemo : MonoBehaviour
{
    public GameObject robotBug;
    public GameObject player;
    public float spawnRate = 10.0f;
    private float time = 9.0f;
    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if(time > spawnRate){
            //Add enemy to the scene
            GameObject bot = Instantiate(robotBug, new Vector3(Random.value * 2000, 850, Random.value * 2000), gameObject.transform.rotation);
            //Tell the bot to go after the player
            Shoot botShootingScript = bot.GetComponent(typeof(Shoot)) as Shoot;
            botShootingScript.player = player;
            Debug.Log("enemy bot added");
            time = 0.0f;
        }
    }
}
