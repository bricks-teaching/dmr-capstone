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
    public AudioClip outOfAmmo;
    public GameObject weapon { get; private set; }
    private int slot = 0;
    public List<GameObject> guns;
    public RawImage crossHair { get; private set; }
    private bool rest = false;
    private float screenX;
    private float screenY;
    private WeaponStats currentWeaponStats;
    private FeedbackController feed;
    public LayerMask missileHomingTargetLayer;

    public void Start() {
        feed = GameObject.FindGameObjectWithTag("HUD").GetComponent<FeedbackController>();
        screenX = Screen.width / 2;
        screenY = Screen.height / 2;
        crossHair = GameObject.FindGameObjectWithTag("Crosshair").GetComponentInChildren<RawImage>();
        fireSound = GetComponent<AudioSource>();
        crossHair.enabled = false;
        LoadWeapon();
    }

    public void Update() {
        // Change weapon
        if (Input.GetKeyDown(Settings.weaponOneKey) && StateManager.purchasedHose) {
            slot = 0;
            if (weapon != null) {
                HolsterWeapon();
            } else {
                LoadWeapon();
            }
        }
        if (Input.GetKeyDown(Settings.weaponTwoKey) && StateManager.purchasedLauncher) {
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

        if (Time.timeScale > 0 && !weaponChanging && Input.GetMouseButton(0)) {  // Firing
            // Make sure player has enough money to fire
            if (PlayerFinance.cashOnHand < currentWeaponStats.bulletCost) {
                if (Input.GetMouseButtonDown(0)) {  // otherwise, the sound is auto fired
                    fireSound.PlayOneShot(outOfAmmo, Settings.volume * 2f);
                }
                return;
            }
            switch (currentWeaponStats.action) {
                case FirearmAction.Automatic:
                    fireAutomatic();
                    break;

                case FirearmAction.Semiautomatic:
                    fireSemiautomatic();
                    break;

                case FirearmAction.SingleShot:
                    // ... would require manual reload action, not implemented.
                    break;
                default:
                    break;
            }
        }

        // If player is currently holstering or drawing a weapon, alter weapon position to animate the process.
        if (drawingWeapon) {
            if ((weapon.transform.parent.position - weapon.transform.position).magnitude > .1) {
                weapon.transform.position = (weapon.transform.position + weapon.transform.parent.position) / 2;
            }
            Vector3 rotationVector = transform.rotation.eulerAngles;
            rotationVector.x += currentWeaponStats.rotation;
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.parent.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
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
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.parent.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
            // After animation has run, get the new weapon
            if (animatedTime > 0.8f) {
                animatedTime = 0f;
                holsteringWeapon = false;
                LoadWeapon();
            }
            animatedTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Continuous fire while button is held down.
    /// </summary>
    private void fireAutomatic() {
        HomingTargets targets = getHomingTargets();
        if (rest) {
            fireBullet(10 - currentWeaponStats.accuracy, currentWeaponStats.strength, 0f, targets);
        } else {
            fireBullet(10 - currentWeaponStats.accuracy, currentWeaponStats.strength, 0.09f, targets);
        }
        rest = !rest;
    }

    /// <summary>
    /// One click, one shot, but no reload action.
    /// </summary>
    private void fireSemiautomatic() {
        if (Input.GetMouseButtonDown(0)) {
            fireBullet(10 - currentWeaponStats.accuracy, currentWeaponStats.strength, 1, getHomingTargets());
        }
    }

    /// <summary>
    /// Sets up homing targets if the projectile has homing enabled.
    /// </summary>
    /// <returns></returns>
    private HomingTargets getHomingTargets() {
        Transform target = null;
        Vector3 localTarget = Vector3.zero;
        RaycastHit hit;
        if (currentWeaponStats.projectileType == ProjectileType.HomingMissile && Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 128, missileHomingTargetLayer)) {
            target = hit.collider.transform;
            localTarget = target.InverseTransformPoint(hit.point);
        }

        return new HomingTargets(target, localTarget);
    }

    /// <summary>
    /// Fire a bullet.
    /// </summary>
    /// <param name="accuracy"></param>
    /// <param name="damage"></param>
    /// <param name="noise">modifier on sound effect volume</param>
    /// <param name="target"></param>
    private void fireBullet(float accuracy, int damage, float noise, HomingTargets target) {
        GameObject bullet;
        Vector3 bulletPosition = currentWeaponStats.projectileSpawnPoint.position;
        if (currentWeaponStats.bulletPool.Count < 2) {
            bullet = Instantiate(currentWeaponStats.bulletPrefab, transform.position, transform.rotation);
        } else {
            bullet = currentWeaponStats.bulletPool.Dequeue();
        }
        BulletBehavior bulletScript = bullet.GetComponent<BulletBehavior>();
        bullet.SetActive(false);

        try {
            bullet.transform.position = bulletPosition;
            bullet.transform.rotation = currentWeaponStats.projectileSpawnPoint.rotation;

            bulletScript.damage = damage;
            bulletScript.target = target.body;
            bulletScript.localizedTarget = target.localizedTarget;
            bulletScript.player = this;

            if (accuracy > 0.1f) {
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

            bullet.SetActive(true);
            currentWeaponStats.bulletPool.Enqueue(bullet);
            StartCoroutine(bulletScript.timeout());
        } catch (System.Exception e) {
            // return bullet to queue in case something errored
            Debug.LogError($"Encountered error while firing: {e}: {e.Message}");
            if (bullet != null) {
                currentWeaponStats.bulletPool.Enqueue(bullet);
            }
        }
        fireSound.PlayOneShot(fireSoundClip, Settings.volume * noise);
        PlayerFinance.cashOnHand -= currentWeaponStats.bulletCost;
        feed.shoot(currentWeaponStats.bulletCost);
    }

    public void LoadWeapon() {
        if (slot == 0 && !StateManager.purchasedHose) {
            return;
        }
        if (slot == 1 && !StateManager.purchasedLauncher) {
            return;
        }
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
        if (guns[slot] == weapon) {
            return;
        }
        holsteringWeapon = true;
        // Hide crossHair while weapon is changing
        crossHair.enabled = false;
    }
}

public struct HomingTargets {
    public Transform body;
    public Vector3 localizedTarget;

    public HomingTargets(Transform transform, Vector3 vector) {
        body = transform;
        localizedTarget = vector;
    }
}
