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
    public AudioClip fireRateSound;
    public AudioClip sprayStart;
    public AudioClip sprayStop;
    public GameObject weapon { get; private set; }
    private int slot = 0;
    public List<GameObject> guns;
    public RawImage crossHair { get; private set; }
    private float screenX => Settings.values.resolutionWidth / 2;
    private float screenY => Settings.values.resolutionHeight / 2;
    public bool loadAfterHolster = true;
    private WeaponStats currentWeaponStats;
    private FeedbackController feed;
    public LayerMask missileHomingTargetLayer;
    private float timeSinceLastShot;
    private bool wasFiringAuto;

    public void Start() {
        feed = GameObject.FindGameObjectWithTag("HUD").GetComponent<FeedbackController>();
        crossHair = GameObject.FindGameObjectWithTag("Crosshair").GetComponentInChildren<RawImage>();
        fireSound = GetComponentInChildren<AudioSource>();
        crossHair.enabled = false;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Dungeon" && !StateManager.inStoryTutorial) {
            this.enabled = false;
        }
        LoadWeapon();
    }

    public void resetFeed() {
        feed = GameObject.FindGameObjectWithTag("HUD").GetComponent<FeedbackController>();
    }

    void OnDisable() {
        stopFiringAuto();
        hideWeapon();
    }

    void OnEnable() {
        // try to pick the first open slot if there was no weapon assigned
        if (weapon == null) {
            if (StateManager.purchasedHose) {
                slot = 0;
            } else if (StateManager.purchasedLauncher) {
                slot = 1;
            }
        }
        LoadWeapon();
    }

    private void hideWeapon() {
        if (weapon != null) {
            weapon.SetActive(false);
            // sometimes, it's null, strangely
            if (crossHair != null) {
                crossHair.enabled = false;
            }
        }
    }

    private void stopFiringAuto() {
        wasFiringAuto = false;
        fireSound?.Stop();
    }

    public void Update() {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Hub" || StateManager.playerDead || StateManager.playerWon) {
            stopFiringAuto();
            return;
        }
        if (StateManager.playerDead || (Input.GetMouseButtonUp(0) && fireSound.isPlaying && wasFiringAuto)) {
            stopFiringAuto();
            fireSound.PlayOneShot(sprayStop);
        }
        // Change weapon
        if (Input.GetKeyDown(Settings.weaponOneKey) && (StateManager.purchasedHose || StateManager.inStoryTutorial)) {
            slot = 0;
            if (weapon != null) {
                HolsterWeapon();
            } else {
                LoadWeapon();
            }
        }
        if (Input.GetKeyDown(Settings.weaponTwoKey) && (StateManager.purchasedLauncher || StateManager.inStoryTutorial)) {
            slot = 1;
            if (weapon != null) {
                HolsterWeapon();
            } else {
                LoadWeapon();
            }
        }
        if (weapon == null) {  // must check after weapon swaps
            return;
        }
        timeSinceLastShot += Time.deltaTime;
        if (currentWeaponStats.showCrosshair && timeSinceLastShot >= currentWeaponStats.fireRate) {
            crossHair.color = new Color(1f, 1f, 1f, 0.8f);
        } else if (currentWeaponStats.showCrosshair) {
            crossHair.color = new Color(1f, 1f, 1f, 0.2f);
        }

        timeSinceLastShot += Time.deltaTime;
        if (currentWeaponStats.showCrosshair && timeSinceLastShot >= currentWeaponStats.fireRate) {
            crossHair.color = new Color(1f, 1f, 1f, 0.8f);
        } else if (currentWeaponStats.showCrosshair) {
            crossHair.color = new Color(1f, 1f, 1f, 0.2f);
        }

        if (Time.timeScale > 0 && !weaponChanging && Input.GetMouseButton(0)) {  // Firing
            // Make sure player has enough money to fire
            if (!StateManager.inStoryTutorial && StateManager.cashOnHand < currentWeaponStats.bulletCost) {
                if (Input.GetMouseButtonDown(0)) {  // otherwise, the sound is auto fired
                    fireSound.PlayOneShot(outOfAmmo, Settings.volume);
                }
                return;
            }
            if (timeSinceLastShot < currentWeaponStats.fireRate && currentWeaponStats.action != FirearmAction.Automatic) {  // auto weapons should fire as fast as they can
                if (Input.GetMouseButtonDown(0)) {  // otherwise, the sound is auto fired
                    fireSound.PlayOneShot(fireRateSound, Settings.volume);
                }
                return;
            }
            if (timeSinceLastShot >= currentWeaponStats.fireRate) {
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
                timeSinceLastShot = 0;
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
                if(loadAfterHolster){
                    LoadWeapon();
                } else {
                    enabled = false;
                }
            }
            animatedTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Continuous fire while button is held down.
    /// </summary>
    private void fireAutomatic() {
        HomingTargets targets = getHomingTargets();
        if (!fireSound.isPlaying) {
            fireSound.volume = Settings.volume;
            fireSound.PlayOneShot(sprayStart);
            fireSound.PlayScheduled(AudioSettings.dspTime + sprayStart.length);
        }
        fireBullet(10 - currentWeaponStats.accuracy, currentWeaponStats.strength, targets);
        wasFiringAuto = true;
    }

    /// <summary>
    /// One click, one shot, but no reload action.
    /// </summary>
    private void fireSemiautomatic() {
        if (Input.GetMouseButtonDown(0)) {
            fireBullet(10 - currentWeaponStats.accuracy, currentWeaponStats.strength, getHomingTargets());
        }
        fireSound.PlayOneShot(fireSoundClip);
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
    /// <param name="target"></param>
    private void fireBullet(float accuracy, int damage, HomingTargets target) {
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
        StateManager.cashOnHand -= currentWeaponStats.bulletCost;
        feed?.shoot(currentWeaponStats.bulletCost);
    }

    public void LoadWeapon() {
        if (slot == 0 && !StateManager.purchasedHose && !StateManager.inStoryTutorial) {
            return;
        }
        if (slot == 1 && !StateManager.purchasedLauncher && !StateManager.inStoryTutorial) {
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
        if (guns[slot] == weapon && loadAfterHolster) {
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
