using System.Collections;
using UnityEngine;

public static class GameController {
    public static GameObject pauseMenu;
    public static GameObject player;
    private static ArrayList bots = new ArrayList();
    private static readonly float groundPosition = GameObject.Find("Ground").transform.position.y;
    private static readonly float groundHeight = GameObject.Find("Ground").GetComponent<Collider>().bounds.size.y;

    public static void spawnBot(GameObject enemy, bool flys){
        GameObject bot = Object.Instantiate(
            enemy, new Vector3(
                player.transform.position.x + Random.Range(0,20),
                flys ? player.transform.position.y + 1.5f : groundPosition + (groundHeight / 2),
                player.transform.position.z + Random.Range(0,20)
            ), player.transform.rotation
        );
        bots.Add(bot);

        //Tell the bot to go after the player
        EnemyMovement botMovement = bot.GetComponent<EnemyMovement>();
        botMovement.player = player;

        EnemyShoot botShootingScript = bot.GetComponent(typeof(EnemyShoot)) as EnemyShoot;
        botShootingScript.player = player;

        //set controller to this script so bot can remove itself from the bot arraylist when it is destroyed
        Debug.Log("enemy bot added");
    }

    public static void removeBot(GameObject bot){
        bots.Remove(bot);
    }
}
