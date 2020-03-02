using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot : MonoBehaviour {
    private bool holsteringWeapon = false;
    private bool drawingWeapon = false;
    private bool weaponChanging = true;
    private float animatedTime;
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    private GameObject weapon = null;
    private int slot = 1;
    private int currentSlot = 0;
    public GameObject gun1;
    public GameObject gun2;
    public GameObject crossHair;
    private Transform weaponPosition;
    private float screenX;
    private float screenY;

    void Start() {
        weaponPosition = transform.GetChild(0);
        screenX = Screen.width / 2;
        screenY = Screen.height / 2;
        crossHair = GameObject.Find("Crosshair");
        fireSound = GetComponent<AudioSource>();
        crossHair.SetActive(false);
        LoadWeapon();
    }

    public void Update() {
        if (Input.GetMouseButtonDown(0) && !weaponChanging && Time.timeScale > 0) {
            WeaponStats weaponScript = weapon.GetComponent(typeof(WeaponStats)) as WeaponStats;
            if (PlayerFinance.cashOnHand < weaponScript.bulletCost) {
                // TODO play sound fx
                return;
            }
            fireBullet(10 - weaponScript.accuracy, weaponScript.strength, weaponScript.bulletPrefab);
            PlayerFinance.cashOnHand -= weaponScript.bulletCost;
        }
        // Change weapon
        if (Input.GetKeyDown("1")) {
            slot = 1;
            if (weapon != null) {
                HolsterWeapon();
            } else {
                LoadWeapon();
            }
        }
        if (Input.GetKeyDown("2")) {
            slot = 2;
            if (weapon != null) {
                HolsterWeapon();
            } else {
                LoadWeapon();
            }
        }
        // If player is currently holstering or drawing a weapon, alter weapon position to animate the process.
        if (drawingWeapon) {
            if ((weaponPosition.position - weapon.transform.position).magnitude > .1) {
                weapon.transform.position = (weapon.transform.position + weaponPosition.position) / 2;
            }
            Vector3 rotationVector = transform.rotation.eulerAngles;
            if (currentSlot == 1) {
                rotationVector.x += 70;
            } else {
                rotationVector.x -= 200;
            }
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
            // After animation has run set weapon changing to false
            if (animatedTime > 0.9f) {
                animatedTime = 0f;
                drawingWeapon = false;
                weaponChanging = false;
                crossHair.SetActive(true);
            }
            animatedTime += Time.deltaTime;
        // ---------------------------------------------------------------------
        } else if (holsteringWeapon) {
            weapon.transform.position -= (transform.up * 5f) + transform.forward;
            Vector3 rotationVector = transform.rotation.eulerAngles;
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
            // After animation has run, get the new weapon
            if (animatedTime > 0.8f) {
                animatedTime = 0f;
                holsteringWeapon = false;
                LoadWeapon();
            }
            animatedTime += Time.deltaTime;
        }
    }

    private void fireBullet(float accuracy, int damage, GameObject bulletPrefab) {
        fireSound.PlayOneShot(fireSoundClip, Settings.volume);
        GameObject bullet = Instantiate(bulletPrefab, weaponPosition.position + weaponPosition.forward, weaponPosition.rotation) as GameObject;
        BulletBehavior bulletScript = bullet.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
        bulletScript.damage = damage;
        Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
        rotationVector.x += ((Random.value * 2) - 1) * accuracy;
        rotationVector.y += ((Random.value * 2) - 1) * accuracy;
        rotationVector.z += ((Random.value * 2) - 1) * accuracy;
        float scatterX = screenX + Random.Range(-accuracy, accuracy);
        float scatterY = screenY + Random.Range(-accuracy, accuracy);
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(scatterX, scatterY, 0));
        bullet.GetComponent<Rigidbody>().velocity = ray.direction * bulletScript.bulletSpeed;
        bullet.transform.rotation = Quaternion.Euler(rotationVector);
        Destroy(bullet, 1f);
    }

    private void LoadWeapon() {
        drawingWeapon = true;
        // Update weapon slot and create weapon object
        currentSlot = slot;
        GameObject choice = gun1;
        if (slot == 2) {
            choice = gun2;
        }
        if (weapon != null) {
            Destroy(weapon);
        }
        weapon = Instantiate(choice, weaponPosition.position - (weaponPosition.up * 2f), weaponPosition.rotation);
        weapon.transform.parent = weaponPosition;
        weaponChanging = true;
    }

    private void HolsterWeapon() {
        // If weapon is already selected, do nothing
        if (slot == currentSlot) {
            return;
        }
        holsteringWeapon = true;
        // Hide crossHair while weapon is changing
        crossHair.SetActive(false);
    }
}
