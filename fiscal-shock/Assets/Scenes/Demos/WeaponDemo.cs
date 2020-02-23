using System.Collections;
using UnityEngine;

// This script is to allow main camera movement in the Weapons Demo scene. It can be deleted once this demo scene is deleted.
public class WeaponDemo : MonoBehaviour
{
    public GameObject robotBug;
    public GameObject spidBot;
    public GameObject player;
    public GameObject gun1;
    public GameObject gun2;
    public int[] gunAmmo = {0, -1, -1};
    public GameObject weapon;
    private int slot = 1;
    private int currentSlot = 0;
    public bool holsteringWeapon = false;
    public bool drawingWeapon = false;
    public float spawnRate = 10.0f;
    private float time = 9.0f;
    private float animatedTime;
    private readonly ArrayList bots = new ArrayList();
    public GameObject gameMenu;
    public GameObject pauseMenu;
    public GameObject crossHair;
    public float volume = 1f;

    public void Start()
    {
        crossHair.SetActive(false);
        LoadWeapon();
    }

    public void FixedUpdate()
    {
        //Bring up pause menu
        if(Input.GetKeyDown("p"))
        {
            if(Time.timeScale == 0){
                Time.timeScale = 1;
            } else {
                Time.timeScale = 0;
            }
            Cursor.lockState = CursorLockMode.None;
            pauseMenu.SetActive(true);
            crossHair.SetActive(false);
        }
        //Change weapon
        if(Input.GetKeyDown("1"))
        {
            slot = 1;
            if(weapon != null)
            {
                HolsterWeapon();
            } else
            {
                LoadWeapon();
            }
        }
        if(Input.GetKeyDown("2"))
        {
            slot = 2;
            if(weapon != null)
            {
                HolsterWeapon();
            } else
            {
                LoadWeapon();
            }
        }
        //If player is currently holstering or drawing a weapon, alter weapon position to animate the process.
        if(drawingWeapon)
        {
            if((player.transform.GetChild(slot - 1).gameObject.transform.position - weapon.transform.position).magnitude > .5)
            {
                weapon.transform.position += player.transform.up * 5f;
            }
            Vector3 rotationVector = player.transform.rotation.eulerAngles;
            rotationVector.x -= 100;
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
            //After animation has run set weapon changing to false
            if(animatedTime > 0.9f)
            {
                animatedTime = 0f;
                drawingWeapon = false;
                PlayerShoot playerShootScript = player.GetComponent(typeof(PlayerShoot)) as PlayerShoot;
                playerShootScript.weaponChanging = false;
                crossHair.SetActive(true);
            }
            animatedTime += Time.deltaTime;
        } else if (holsteringWeapon)
        {
            // Debug.Log(animatedTime);
            weapon.transform.position -= (player.transform.up * 5f) + player.transform.forward;
            Vector3 rotationVector = player.transform.rotation.eulerAngles;
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
            //After animation has run get the new weapon
            if(animatedTime > 0.8f)
            {
                animatedTime = 0f;
                holsteringWeapon = false;
                LoadWeapon();
            }
            animatedTime += Time.deltaTime;
        }
        //Spawn a new bot when time passed reaches spawnRate
        time += Time.deltaTime;
        if(time > spawnRate){
            GameObject bot = Instantiate(robotBug, new Vector3(Random.value * 2000, player.transform.position.y, Random.value * 2000), gameObject.transform.rotation);
            bots.Add(bot);
            //Tell the bot to go after the player
            EnemyShoot botShootingScript = bot.GetComponent(typeof(EnemyShoot)) as EnemyShoot;
            botShootingScript.player = player;
            botShootingScript.volume = volume;
            EnemyHealth botDamageScript = bot.GetComponent(typeof(EnemyHealth)) as EnemyHealth;
            botDamageScript.volume = volume;
            //set controller to this script so bot can remove itself from the bot arraylist when it is destroyed
            botDamageScript.controller = gameObject;
            Debug.Log("enemy bot added");
            time = 0.0f;
        }
    }

    public void removeBot(GameObject bot){
        bots.Remove(bot);
    }

    public void ChangeVolume(float vol)
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

    private void LoadWeapon()
    {
        drawingWeapon = true;
        //Update weapon slot and create weapon object
        currentSlot = slot;
        GameObject choice = gun1;
        if(slot == 2)
        {
            choice = gun2;
        }
        if(weapon != null){
            Destroy(weapon);
        }
        weapon = Instantiate(choice, player.transform.GetChild(slot - 1).gameObject.transform.position - (player.transform.up * 80f), player.transform.rotation);
        if(gunAmmo[slot] > -1){
            WeaponStats statsScript = weapon.GetComponent(typeof(WeaponStats)) as WeaponStats;
            statsScript.ammo = gunAmmo[slot];
        }
        weapon.transform.parent = player.transform;
        //Attach weapon to shooting script
        PlayerShoot playerShootScript = player.GetComponent(typeof(PlayerShoot)) as PlayerShoot;
        playerShootScript.weapon = weapon;
        playerShootScript.weaponChanging = true;
    }

    private void HolsterWeapon()
    {
        //If weapon is already selected, do nothing
        if(slot == currentSlot)
        {
            return;
        }
        holsteringWeapon = true;
        //Hide crossHair while weapon is changing
        crossHair.SetActive(false);
        //Update ammo left
        WeaponStats statsScript = weapon.GetComponent(typeof(WeaponStats)) as WeaponStats;
        gunAmmo[currentSlot] = statsScript.ammo;
        //Disable ability for player to shoot
        PlayerShoot playerShootScript = player.GetComponent(typeof(PlayerShoot)) as PlayerShoot;
        playerShootScript.weapon = weapon;
        playerShootScript.weaponChanging = true;
    }
}
