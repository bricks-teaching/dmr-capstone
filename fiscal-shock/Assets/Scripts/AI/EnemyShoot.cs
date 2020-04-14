using UnityEngine;

//This script allows enemy bots to fire weapons.
public class EnemyShoot : MonoBehaviour {
    /// <summary>
    /// Reference to the player. Set at runtime.
    /// </summary>
    public GameObject player { get; private set; }

    [Tooltip("Optional prefab for the bullet used. Only the physics of the bullet is used; damage is taken from this script. If empty, the bot is considered a melee attacker, and should have its range set appropriately.")]
    public GameObject bulletPrefab;

    [Tooltip("Sound the bot makes when attacking.")]
    public AudioClip fireSoundClip;

    [Tooltip("Reference to this bot's own enemy movement script.")]
    public EnemyMovement enemyMovement;

    [Tooltip("Reference to this bot's animation manager. Script already on the gameobject should just be dragged and dropped onto this slot.")]
    public AnimationManager animationManager;

    [Tooltip("Point to spawn projectiles at while attacking, or the center of a spherecheck for melee attacks.")]
    public Transform projectileSpawnPoint;

    [Tooltip("Amount of damage done per shot. Slightly randomized.")]
    public float attackDamage = 10;

    [Tooltip("Jitter magnitude applied to bot's aim. Higher numbers indicate greater inaccuracy.")]
    public float aimJitter = 10;

    [Tooltip("How close the bot must be to begin firing.")]
    public float attackRange = 6f;

    [Tooltip("Time, in seconds, between firing. Does not take animations into account.")]
    public float attackDelay = 1.7f;

    [Tooltip("How long after the attack animation starts that the projectile should actually be fired")]
    public float attackAnimationDelay = 1f;

    [Tooltip("Whether this enemy can fire while moving, or must stop to fire.")]
    public bool runAndGun = false;

    [Tooltip("Whether this bot has seen the player and therefore can start the attack routine. Set in EnemyMovement, since that already tracks the player's distance.")]
    public bool spottedPlayer;

    /// <summary>
    /// Reference to this bot's own audio source object. Needed to play sounds.
    /// </summary>
    private AudioSource fireSound;

    /// <summary>
    /// How long since the last successful attack was launched.
    /// <seealso>attackDelay</seealso>
    /// </summary>
    private float timeSinceLastAttack = 0.0f;

    /// <summary>
    /// Reference to the length of the attack animation. Enemies that can't
    /// move and shoot need to wait this long to start moving again.
    /// </summary>
    private float attackAnimationLength;

    /// <summary>
    /// Whether this bot is firing. Stops the firing routine from being
    /// started prematurely.
    /// </summary>
    private bool isFiring = false;

    /// <summary>
    /// LayerMask for the player, used for physics checks
    /// </summary>
    private int playerMask;

    /// <summary>
    /// Reference to player health script. Used to deal damage with melee
    /// attacks
    /// </summary>
    private PlayerHealth playerHealth;

    void Start() {
        fireSound = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update() {
        if (player == null || !spottedPlayer) { return; }

        float distance = enemyMovement.getDistanceFromPlayer();

        if (distance <= attackRange) {
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack > (attackDelay * Random.Range(0.75f, 1.40f)) && !isFiring) {
                StartCoroutine(fireBullet(10 - aimJitter, attackDamage));
            }
        }
    }

    private System.Collections.IEnumerator fireBullet(float accuracy, float damage) {
        isFiring = true;
        if (!runAndGun) {
            enemyMovement.enabled = false;
        }
        attackAnimationLength = animationManager.playAttackAnimation();
        yield return new WaitForSeconds(attackAnimationDelay);
        fireSound.PlayOneShot(fireSoundClip, Settings.volume);

        // Instantiate the projectile
        // Assumes bot is facing the player, so fire in that direction
        if (bulletPrefab != null) {
            GameObject bullet = Instantiate(
                bulletPrefab,
                projectileSpawnPoint.position,
                transform.rotation);
            bullet.SetActive(false);
            bullet.transform.parent = transform;
            bullet.transform.LookAt(player.transform);
            bullet.name = $"{gameObject.name}'s {bulletPrefab.name}";
            BulletBehavior bulletScript = bullet.GetComponent<BulletBehavior>();
            if (bullet.tag == "Missile") {
                bulletScript.target = player.transform;
            }
            bullet.tag = "Enemy Projectile";
            bulletScript.damage = damage * Random.Range(0.8f, 1.2f);

            // Fire the bullet and apply accuracy
            Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
            rotationVector.x += ((Random.value * 2) - 1) * accuracy;
            rotationVector.y += ((Random.value * 2) - 1) * accuracy;
            rotationVector.z += ((Random.value * 2) - 1) * accuracy;
            bullet.transform.rotation = Quaternion.Euler(rotationVector);
            bullet.SetActive(true);
            Destroy(bullet, bulletScript.bulletLifetime);
        } else {  // Do a melee attack using the projectile spawn point as the epicenter
            if (Physics.CheckSphere(projectileSpawnPoint.position, attackRange, playerMask)) {
                playerHealth.takeDamage(damage);
            }
        }

        if (!runAndGun) {
            yield return new WaitForSeconds(attackAnimationLength - attackAnimationDelay);
            enemyMovement.enabled = true;
        }

        isFiring = false;
        timeSinceLastAttack = 0;
        yield return null;
    }
}
