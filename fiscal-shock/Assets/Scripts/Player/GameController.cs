using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameController
{
    public static GameObject pauseMenu;
    public static GameObject player;
    public static float volume = .3f;
    private static ArrayList bots = new ArrayList();

    public static void spawnBot(GameObject enemy, bool flys){
        GameObject bot = Object.Instantiate(enemy, new Vector3(player.transform.position.x + Random.value * 20, player.transform.position.y + (flys ? 1.5f : 0), player.transform.position.z + Random.value * 20), player.transform.rotation);
        bots.Add(bot);
        //Tell the bot to go after the player
        EnemyShoot botShootingScript = bot.GetComponent(typeof(EnemyShoot)) as EnemyShoot;
        botShootingScript.player = player;
        botShootingScript.volume = volume;
        EnemyHealth botDamageScript = bot.GetComponent(typeof(EnemyHealth)) as EnemyHealth;
        botDamageScript.volume = volume;
        //set controller to this script so bot can remove itself from the bot arraylist when it is destroyed
        Debug.Log("enemy bot added");
    }
    public static void removeBot(GameObject bot){
        bots.Remove(bot);
    }

     public static void ChangeVolume(float vol)
    {
        volume = vol;
        //Go through all the scripts attached to player and the bots in the scene and update the volume
        PlayerShoot playerShootScript = player.GetComponent(typeof(PlayerShoot)) as PlayerShoot;
        playerShootScript.volume = vol;
        for(int i = 0; i < bots.Count; i++)
        {
            GameObject bot = bots[i] as GameObject;
            EnemyShoot botShootingScript = bot.GetComponent(typeof(EnemyShoot)) as EnemyShoot;
            botShootingScript.volume = vol;
            EnemyHealth botDamageScript = bot.GetComponent(typeof(EnemyHealth)) as EnemyHealth;
            botDamageScript.volume = vol;
        }
    }
}
