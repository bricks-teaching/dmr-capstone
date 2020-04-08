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
    public GameObject weapon { get; private set; }
    private int slot = 0;
    public List<GameObject> guns;
    public RawImage crossHair { get; private set; }
    private bool rest = false;
    public RaycastHit hit;
    private ArrayList missiles = new ArrayList();
    private float screenX;
    private float screenY;
    public bool loadAfterHolster = true;
    private WeaponStats currentWeaponStats;
    private FeedbackController feed;
    public bool tutorial = true;


    public void Start() {
        feed = GameObject.FindGameObjectWithTag("HUD").GetComponent<FeedbackController>();
        screenX = Screen.width / 2;
        screenY = Screen.height / 2;
        crossHair = GameObject.FindGameObjectWithTag("Crosshair").GetComponentInChildren<RawImage>();
        fireSound = GetComponent<AudioSource>();
        crossHair.enabled = false;
        Debug.Log("feed is " + feed);
        LoadWeapon();
    }

    public void resetFeed(){
        feed = GameObject.FindGameObjectWithTag("HUD").GetComponent<FeedbackController>();
    }

    public void Update() {
        // Change weapon
        if (Input.GetKeyDown(Settings.weaponOneKey)) {
            slot = 0;
            if (weapon != null) {
                HolsterWeapon();
            } else {
                LoadWeapon();
            }
        }
        if (Input.GetKeyDown(Settings.weaponTwoKey)) {
        slot = 1;
            if (weapon != null) {
                HolsterWeapon();
            } else {
                LoadWeapon();
            }
        }
        if (weapon == null) {
            return;
        }
        foreach (GameObject missile in missiles) {
            BulletBehavior bulletScript = missile.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            bulletScript.rb.velocity = (bulletScript.target.position - missile.transform.position).normalized * bulletScript.bulletSpeed;
        }
        if (currentWeaponStats.continuous) {
            if (Input.GetMouseButtonDown(0) && !weaponChanging && Time.timeScale > 0)
            {
                fireSound.PlayOneShot(fireSoundClip, 0.5f * Settings.volume);
            }
            if (Input.GetMouseButton(0) && !weaponChanging && Time.timeScale > 0)
            {
				if (PlayerFinance.cashOnHand < currentWeaponStats.bulletCost) {
					// TODO play sound fx
					return;
				}
                if (rest) {
                    fireBullet(10 - currentWeaponStats.accuracy, currentWeaponStats.strength, currentWeaponStats.bulletPrefab, 0f, null);
                        PlayerFinance.cashOnHand -= currentWeaponStats.bulletCost;
                    feed?.shoot(currentWeaponStats.bulletCost);
                } else {
                    fireBullet(10 - currentWeaponStats.accuracy, currentWeaponStats.strength, currentWeaponStats.bulletPrefab, 0.09f, null);
                    PlayerFinance.cashOnHand -= currentWeaponStats.bulletCost;
                    feed?.shoot(currentWeaponStats.bulletCost);
                }
                rest = !rest;
            }
        } else {
            if (Input.GetMouseButtonDown(0) && !weaponChanging && Time.timeScale > 0)
            {
				if (PlayerFinance.cashOnHand < currentWeaponStats.bulletCost) {
					// TODO play sound fx
					return;
				}
                Transform target = null;
                if (currentWeaponStats.missile && Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity)){
                    //Debug.Log(hit.collider);
                    //Instantiate(debugger, transform.position + (transform.TransformDirection(Vector3.forward) * hit.distance), transform.rotation);
                    target = hit.collider.transform;
                }
                fireBullet(10 - currentWeaponStats.accuracy, currentWeaponStats.strength, currentWeaponStats.bulletPrefab, 1, target);
                PlayerFinance.cashOnHand -= currentWeaponStats.bulletCost;
                feed?.shoot(currentWeaponStats.bulletCost);
            }
        }
        // If player is currently holstering or drawing a weapon, alter weapon position to animate the process.
        if (drawingWeapon) {
            if ((weapon.transform.parent.position - weapon.transform.position).magnitude > .1) {
                weapon.transform.position = (weapon.transform.position + weapon.transform.parent.position) / 2;
            }
            Vector3 rotationVector = transform.rotation.eulerAngles;
            rotationVector.x += currentWeaponStats.rotation;
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
            // After animation has run set weapon changing to false
            if (animatedTime > 0.9f) {
                animatedTime = 0f;
                drawingWeapon = false;
                weaponChanging = false;
                crossHair.enabled = currentWeaponStats.showCrosshair;
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
                if(loadAfterHolster){
                    LoadWeapon();
                } else {
                    enabled = false;
                }
            }
            animatedTime += Time.deltaTime;
        }
    }

    private void fireBullet(float accuracy, int damage, GameObject bulletPrefab, float noise, Transform target) {
        fireSound.PlayOneShot(fireSoundClip, Settings.volume * noise);
        GameObject bullet;
        Vector3 bulletPosition = weapon.transform.parent.position + weapon.transform.parent.forward;

        if (currentWeaponStats.usingPool) {
            bullet = currentWeaponStats.bulletPool.Dequeue();
            bullet.transform.position = bulletPosition;
            bullet.transform.rotation = weapon.transform.parent.rotation;
            bullet.SetActive(true);
        } else {
            bullet = Instantiate(bulletPrefab, bulletPosition, weapon.transform.parent.rotation);
        }

        BulletBehavior bulletScript = bullet.GetComponent(typeof(BulletBehavior)) as BulletBehavior;

        if (currentWeaponStats.usingPool) {
            bulletScript.Start();
        }

        bulletScript.damage = damage;
        if (target != null){
            bulletScript.target = target;
            bulletScript.player = this;
            missiles.Add(bullet);
        } else if (accuracy > 0.1f) {
            Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
            rotationVector.x += ((Random.value * 2) - 1) * accuracy;
            rotationVector.y += ((Random.value * 2) - 1) * accuracy;
            rotationVector.z += ((Random.value * 2) - 1) * accuracy;
            float scatterX = screenX + Random.Range(-accuracy, accuracy);
            float scatterY = screenY + Random.Range(-accuracy, accuracy);
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(scatterX, scatterY, 0));
            bulletScript.rb.velocity = ray.direction * bulletScript.bulletSpeed;
            bullet.transform.rotation = Quaternion.Euler(rotationVector);
        }

        if (currentWeaponStats.usingPool) {
            currentWeaponStats.bulletPool.Enqueue(bullet);
            StartCoroutine(bulletScript.timeout());
        } else {
            Destroy(bullet, 2f);
        }
    }

    public void LoadWeapon() {
        drawingWeapon = true;
        // Update weapon slot and enable weapon object
        weapon?.SetActive(false);
        weapon = guns[slot];
        weapon?.SetActive(true);
        currentWeaponStats = weapon.GetComponent(typeof(WeaponStats)) as WeaponStats;
        weaponChanging = true;
    }

    public void HolsterWeapon() {
        // If weapon is already selected, do nothing
        if (guns[slot] == weapon && loadAfterHolster) {
            return;
        }
        holsteringWeapon = true;
        // Hide crossHair while weapon is changing
        crossHair.enabled = false;
    }

    public void removeMissile(GameObject missile){
        missiles.Remove(missile);
    }

    public void turnOff(){
        loadAfterHolster = false;
        HolsterWeapon();
    }

}
