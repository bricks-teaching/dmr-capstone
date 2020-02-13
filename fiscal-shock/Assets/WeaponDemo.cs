using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is to allow main camera movement in the Weapons Demo scene. It can be deleted once this demo scene is deleted.
public class WeaponDemo : MonoBehaviour
{
    public GameObject robotBug;
    public GameObject player;
    public GameObject gun1;
    public GameObject gun2;
    public GameObject weapon;
    private int slot = 0;
    private int currentSlot = 0;
    public bool holsteringWeapon = false;
    public bool drawingWeapon = false;
    public float spawnRate = 10.0f;
    private float time = 9.0f;
    private float weaponChangeTime = 0f;
    // Update is called once per frame
    void Update()
    {
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
        if(drawingWeapon)
        {
            //Debug.Log("downDist " + (player.transform.GetChild(0).gameObject.transform.position - weapon.transform.position).magnitude);
            if((player.transform.GetChild(0).gameObject.transform.position - weapon.transform.position).magnitude > .5)
            {
                Debug.Log("downDist " + (player.transform.GetChild(0).gameObject.transform.position - weapon.transform.position).magnitude);
                weapon.transform.position += player.transform.up * 5f;
            } 
            Vector3 rotationVector = player.transform.rotation.eulerAngles;
            rotationVector.x -= 100;
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 5);
            if(weaponChangeTime > 1f)
            {
                weaponChangeTime = 0f;
                drawingWeapon = false;
                PlayerShoot playerShootScript = player.GetComponent(typeof(PlayerShoot)) as PlayerShoot;
                playerShootScript.weaponChanging = false;
                Debug.Log("Weapon changed");
            }
            weaponChangeTime += Time.deltaTime;
        } else if (holsteringWeapon)
        {
            weapon.transform.position -= player.transform.up * 5f;
            Vector3 rotationVector = player.transform.rotation.eulerAngles;
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 5);
            if(weaponChangeTime > 1f)
            {
                weaponChangeTime = 0f;
                holsteringWeapon = false;
                PlayerShoot playerShootScript = player.GetComponent(typeof(PlayerShoot)) as PlayerShoot;
                playerShootScript.weaponChanging = false;
                Debug.Log("CallLoadWeapon");
                LoadWeapon();
            }
            weaponChangeTime += Time.deltaTime;
        }
        /*
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
        */
    }

    void LoadWeapon()
    {
        currentSlot = slot;
        GameObject choice = gun1;
        if(slot == 2)
        {
            choice = gun2;
        }
        drawingWeapon = true;
        weapon = Instantiate(choice, player.transform.GetChild(0).gameObject.transform.position - (player.transform.up * 80f), player.transform.rotation);
        weapon.transform.parent = player.transform;
        PlayerShoot playerShootScript = player.GetComponent(typeof(PlayerShoot)) as PlayerShoot;
        playerShootScript.weapon = weapon;
        playerShootScript.weaponChanging = true;
    }

    void HolsterWeapon()
    {
        if(slot == currentSlot)
        {
            return;
        }
        holsteringWeapon = true;
    }
}
