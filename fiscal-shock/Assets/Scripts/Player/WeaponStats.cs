using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FiscalShock.Procedural;

//This script holds information about a weapon
public class WeaponStats : MonoBehaviour {
    [Header("Information")]
    public string prefix;
    public string weaponName;
    public string suffix;
    public string weaponFamily;

    [Tooltip("Base price of this weapon.")]
    public float price;
    /// <summary>
    /// Buyback price is currently hardcoded to 15% of the original cost.
    /// </summary>
    public float buybackPrice => (float)System.Math.Round(price * 0.15f, 2);

    /// <summary>
    /// Full display name of weapon.
    /// </summary>
    public string fullName => $"{prefix} {weaponName} {suffix}";

    [Header("Graphics")]
    [Tooltip("Graphic displayed in the shop and player inventory.")]
    public Sprite image;

    [Header("Statistics")]
    [Tooltip("Base bullet damage.")]
    public float strength = 10;

    [Tooltip("Cost per trigger pull.")]
    public float bulletCost = 1;

    [Tooltip("Lower jitter means greater accuracy.")]
    public float deviation = 6;

    [Tooltip("Time, in seconds, between attacks.")]
    public float fireDelay = 0;  // 0 = no delay

    [Tooltip("Value to help position weapons when drawn.")]
    public float rotation = 0;

    [Tooltip("Trigger action of this weapon. Automatic weapons continue firing while the fire button is held down. Semiautomatic weapons fire with each button press. Single-shot is not implemented.")]
    // Semiauto could still require a reload if clip sizes are implemented
    // Single-shot would perform similarly to a bolt-action rifle, as in needing a manual reload after every shot.
    public FirearmAction action;

    [Tooltip("Type of projectile this weapon fires")]
    public ProjectileType projectileType;
    [Tooltip("Layer that missiles fired from this would seek. Should just be enemy.")]
    public LayerMask missileHomingTargetLayer;
    [Tooltip("Whether this weapon displays the crosshair.")]
    public bool showCrosshair = true;

    [Header("Components")]
    [Tooltip("Bullet prefab used. Physical bullet properties, including mass, whether it is affected by gravity, and the speed of the bullet are not altered from the prefab.")]
    public GameObject bulletPrefab;
    [Tooltip("Location on the gun that projectiles from which projectiles emerge. Typically the muzzle of the gun, or slightly beyond it (due to bullet sizes).")]
    public Transform projectileSpawnPoint;

    [Tooltip("Audio source to use for this weapon. If this is an automatic weapon, the audio source should have the looping audio clip attached.")]
    public AudioSource gunAudioSource;

    [Tooltip("Sound this weapon makes when firing.")]
    public AudioClip fireSound;

    [Tooltip("[Automatic] Sound played when automatic firing begins.")]
    public AudioClip autoStart;

    [Tooltip("[Automatic] Sound played when automatic firing ends.")]
    public AudioClip autoStop;

    /// <summary>
    /// Object pool for this weapon's bullets.
    /// </summary>
    public Queue<GameObject> bulletPool = new Queue<GameObject>();
    /// <summary>
    /// Reference to the game object owning the bullet pool. Used to clean up
    /// all related parts of a gun when it's sold.
    /// </summary>
    public GameObject bulletPoolObject { get; set; }
    /// <summary>
    /// Size of the crosshair, relative to the screen.
    /// </summary>
    private float crosshairRelativeX => crosshair.rect.width / Screen.width;
    /// <summary>
    /// Size of the crosshair, relative to the screen.
    /// </summary>
    private float crosshairRelativeY => crosshair.rect.height / Screen.height;

    /// <summary>
    /// Raycast origin. Should be the camera's transform, not this weapon's!
    /// Otherwise, the crosshair is inaccurate.
    /// </summary>
    private Transform raycastOrigin;
    /// <summary>
    /// Reference to the crosshair. Used in accuracy calculations to ensure that
    /// bullet trajectories match the crosshair size and location.
    /// </summary>
    private RectTransform crosshair;

    /// <summary>
    /// Enum to string for FirearmAction
    /// </summary>
    /// <returns></returns>
    public string actionToString() {
        string actions;
        switch (action) {
            case FirearmAction.Automatic:
                actions = "Automatic"; break;
            case FirearmAction.Semiautomatic:
                actions = "Semiautomatic"; break;
            default:
                actions = "Dummy Action"; break;
        }
        return actions;
    }

    /// <summary>
    /// Enum to string for ProjectileType
    /// </summary>
    public string projectileTypeToString() {
        string projtype;
        switch (projectileType) {
            case ProjectileType.Bullet:
                projtype = "Bullet"; break;
            case ProjectileType.HomingMissile:
                projtype = "Missile"; break;
            default:
                projtype = "Dummy Proj"; break;
        }
        return projtype;
    }

    /// <summary>
    /// Debug string
    /// </summary>
    public override string ToString() {
        return $"Name: {fullName}, Family: {weaponFamily}, Value: {price}, Strength: {strength}, Deviation: {deviation}, Delay: {fireDelay}, Bullet Cost: {bulletCost}, Action: {actionToString()}, Projectile: {projectileTypeToString()}, Crosshair: {showCrosshair}";
    }

    void Start() {
        if (bulletPrefab == null) {
            return;
        }
        BulletBehavior bb = bulletPrefab.GetComponent<BulletBehavior>();
        if (bb.poolSize < 1) {
            Debug.LogError($"Bullet prefab must have a pool size for player-fired bullets!");
        }
        GameObject pool = new GameObject();
        pool.name = $"{gameObject.name}'s Projectile Pool";
        string tag = "";
        switch (projectileType) {
            case ProjectileType.Bullet:
                tag = "Bullet"; break;
            case ProjectileType.HomingMissile:
                tag = "Missile"; break;
        }
        for (int i = 0; i < bb.poolSize; ++i) {
            GameObject boolet = Instantiate(bulletPrefab, bulletPrefab.transform.position, bulletPrefab.transform.rotation);
            boolet.transform.parent = pool.transform;
            boolet.SetActive(false);
            bulletPool.Enqueue(boolet);
            boolet.tag = tag;
            // enable ricochet for non-auto bullets
            boolet.GetComponent<BulletBehavior>().ricochetEnabled = (action != FirearmAction.Automatic);
        }
        DontDestroyOnLoad(pool);
        StateManager.singletons.Add(pool);
        bulletPoolObject = pool;

        raycastOrigin = GameObject.FindGameObjectWithTag("MainCamera").transform;
        crosshair = GameObject.FindGameObjectWithTag("Crosshair").GetComponent<RectTransform>();
    }

    /// <summary>
    /// Sets up homing targets if the projectile has homing enabled.
    /// WARNING: Homing missiles currently bypass accuracy checks. This raycast
    /// should really be affected by deviation. (Would probably be almost the
    /// same code as the rotation vector in fireBullet)
    /// </summary>
    /// <returns></returns>
    private HomingTargets getHomingTargets() {
        Transform target = null;
        Vector3 localTarget = Vector3.zero;
        RaycastHit hit;
        if (projectileType == ProjectileType.HomingMissile && Physics.Raycast(raycastOrigin.position, raycastOrigin.TransformDirection(Vector3.forward), out hit, 128, missileHomingTargetLayer)) {
            // Get enemy target
            target = hit.collider.transform;
            localTarget = target.InverseTransformPoint(hit.point);
        }

        return new HomingTargets(target, localTarget);
    }

    /// <summary>
    /// Fires a bullet based on the firearm action. Also mutates the given
    /// PlayerShoot state.
    /// </summary>
    /// <param name="state">current state of PlayerShoot</param>
    /// <returns>updated state</returns>
    public PlayerFiringState fire(PlayerFiringState state) {
        switch (action) {
            case FirearmAction.Automatic:
                fireAutomatic(state);
                break;

            case FirearmAction.Semiautomatic:
                fireSemiautomatic(state);
                break;

            case FirearmAction.SingleShot:
                // ... would require manual reload action, not implemented.
                break;
            default:
                break;
        }
        state.nextLossCost = bulletCost;
        state.needToUpdateLossFeed = true;
        return state;
    }

    /// <summary>
    /// Continuous fire while button is held down.
    /// </summary>
    private void fireAutomatic(PlayerFiringState state) {
        if (fireDelay < 0.03f) {
            if (!gunAudioSource.isPlaying) {
                gunAudioSource.volume = Settings.volume;
                gunAudioSource.PlayOneShot(autoStart, Settings.volume);
                gunAudioSource.PlayScheduled(AudioSettings.dspTime + autoStart.length);
            }
        } else {
            gunAudioSource.PlayOneShot(fireSound, Settings.volume);
        }
        fireBullet();
        state.firingAutomatic = true;
    }

    /// <summary>
    /// One click, one shot, but no reload action.
    /// </summary>
    private void fireSemiautomatic(PlayerFiringState state) {
        if (!state.alreadyFired) {
            fireBullet();
            gunAudioSource.PlayOneShot(fireSound, Settings.volume);
        }
        state.alreadyFired = true;
    }

    /// <summary>
    /// Fire a bullet. Only works for weapons that emit a single bullet at a
    /// time. A shotgun or melee weapon would require a different function.
    /// </summary>
    private void fireBullet() {
        HomingTargets target = getHomingTargets();
        GameObject bullet;
        Vector3 bulletPosition = projectileSpawnPoint.position;
        if (bulletPool.Count < 2) {
            bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        } else {
            bullet = bulletPool.Dequeue();
        }
        BulletBehavior bulletScript = bullet.GetComponent<BulletBehavior>();
        bulletScript.enabled = false;
        bullet.SetActive(false);

        try {
            bullet.transform.position = bulletPosition;
            bullet.transform.rotation = projectileSpawnPoint.rotation;

            bulletScript.damage = strength;
            bulletScript.target = target.body;
            bulletScript.localizedTarget = target.localizedTarget;

            if (bulletScript.target == null) {
                // Apply rotation changes based on deviation
                // Since bullets travel based on their forward direction,
                // this is the application of accuracy.
                float xsd = deviation * crosshairRelativeX;
                float ysd = deviation * crosshairRelativeY;
                Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
                rotationVector.x += Gaussian.next(-xsd, xsd);
                rotationVector.y += Gaussian.next(-ysd, ysd);
                rotationVector.z += Gaussian.next(-xsd, xsd);

                bullet.transform.rotation = Quaternion.Euler(rotationVector);
            } else {
                // Homing bullets will handle orientation
                bulletScript.seekingTarget = true;
            }

            bulletScript.enabled = true;
            bullet.SetActive(true);
            bulletPool.Enqueue(bullet);
            StartCoroutine(bulletScript.timeout());
        } catch (System.Exception e) {
            // return bullet to queue in case something errored
            Debug.LogError($"Encountered error while firing: {e}: {e.Message}");
            if (bullet != null) {
                bulletPool.Enqueue(bullet);
            }
        }
        StateManager.cashOnHand -= bulletCost;
    }
}

/// <summary>
/// How trigger pulls are handled on this weapon.
/// </summary>
public enum FirearmAction {
    SingleShot,  // Manual reload after every shot (not implemented)
    Automatic,  // Hold down the trigger to keep shooting
    Semiautomatic  // Release trigger after each pull so the firearm can reload itself, no manual reloads needed between shots
}

/// <summary>
/// Types of projectiles
/// </summary>
public enum ProjectileType {
    Bullet,  // Bullet simply travels forward from its initial trajectory (altered by accuracy)
    HomingMissile  // Bullet seeks out enemy targets, regardless of accuracy
}

/// <summary>
/// Data about a target being sought by a homing missile
/// </summary>
public struct HomingTargets {
    /// <summary>
    /// Main body of the target hit
    /// </summary>
    public Transform body;
    /// <summary>
    /// Localized point on the body where the raycast hit occurred.
    /// The bullet will travel to this point, not the origin of the body.
    /// </summary>
    public Vector3 localizedTarget;

    public HomingTargets(Transform transform, Vector3 vector) {
        body = transform;
        localizedTarget = vector;
    }
}
