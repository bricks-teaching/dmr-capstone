using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot : MonoBehaviour
{
    private bool holsteringWeapon = false;
    private bool drawingWeapon = false;
    private bool weaponChanging = true;
    private float animatedTime;
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    private GameObject weapon = null;
    public GameObject controller;
    public float volume = 1f;
    private int slot = 1;
    private int currentSlot = 0;
    public GameObject gun1;
    public GameObject gun2;
    public GameObject crossHair;
    // Start is called before the first frame update

    void Start()
    {
        GameController.player = gameObject;
        fireSound = GetComponent<AudioSource>();
        crossHair.SetActive(false);
        LoadWeapon();
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && !weaponChanging && Time.timeScale > 0)
        {
            WeaponStats weaponScript = weapon.GetComponent(typeof(WeaponStats)) as WeaponStats;
            fireBullet(10 - weaponScript.accuracy, weaponScript.strength, weaponScript.bulletPrefab);
            PlayerFinance.cashOnHand -= weaponScript.bulletCost;
        }
        //Bring up pause menu
        if(Input.GetKeyDown("p"))
        {
            if(Time.timeScale == 0){
                Time.timeScale = 1;
            } else {
                Time.timeScale = 0;
            }
            Cursor.lockState = CursorLockMode.None;
            GameController.pauseMenu.SetActive(true);
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
            if((transform.GetChild(0).gameObject.transform.position - weapon.transform.position).magnitude > .1)
            {
                weapon.transform.position = (weapon.transform.position + transform.GetChild(0).gameObject.transform.position) / 2;
            }
            Vector3 rotationVector = transform.rotation.eulerAngles;
            if(currentSlot == 1){
                rotationVector.x -= 10;
            } else {
                 rotationVector.x -= 100;
            }
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
            //After animation has run set weapon changing to false
            if(animatedTime > 0.9f)
            {
                animatedTime = 0f;
                drawingWeapon = false;
                weaponChanging = false;
                crossHair.SetActive(true);
            }
            animatedTime += Time.deltaTime;
        } else if (holsteringWeapon)
        {
            weapon.transform.position -= (transform.up * 5f) + transform.forward;
            Vector3 rotationVector = transform.rotation.eulerAngles;
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
    }

    private void fireBullet(float accuracy, int damage, GameObject bulletPrefab)
    {
        fireSound.PlayOneShot(fireSoundClip, volume);
        GameObject bullet = Instantiate(bulletPrefab, gameObject.transform.position + gameObject.transform.forward * 2, gameObject.transform.rotation) as GameObject;
        BulletBehavior bulletScript = bullet.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
        bulletScript.damage = damage;
        Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
        rotationVector.x += ((Random.value * 2) - 1) * accuracy;
        rotationVector.y += ((Random.value * 2) - 1) * accuracy;
        rotationVector.z += ((Random.value * 2) - 1) * accuracy;
        bullet.transform.rotation = Quaternion.Euler(rotationVector);
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
        weapon = Instantiate(choice, transform.GetChild(0).gameObject.transform.position - (transform.up * 2f), transform.rotation);
        weapon.transform.parent = transform;
        weaponChanging = true;
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
    }
}
