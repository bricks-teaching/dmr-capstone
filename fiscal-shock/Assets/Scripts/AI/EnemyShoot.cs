using UnityEngine;

//This script allows enemy bots to fire weapons.
public class EnemyShoot : MonoBehaviour {
    public GameObject bulletPrefab;
    public GameObject player { get; set; }
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    private float time = 0.0f;

    [Tooltip("Amount of damage done per shot.")]
    public int botDamage = 10;

    [Tooltip("How accurately the bot fires.")]
    public int botAccuracy = 10;

    [Tooltip("How close the bot must be to begin firing.")]
    public float botRange = 6f;

    [Tooltip("Fire rate of the bot.")]
    public float botRate = 1.7f;

    public EnemyMovement enemyMovement;
    public bool spottedPlayer;
    public AnimationManager animationManager;

    [Tooltip("Point to spawn projectiles at while attacking")]
    public Transform projectileSpawnPoint;
    [Tooltip("How long after the attack animation starts that the projectile should actually be fired")]
    public float attackAnimationDelay = 1f;
    [Tooltip("Whether this enemy can fire while moving, or must stop to fire.")]
    public bool runAndGun = false;
    private float attackAnimationLength => animationManager.attackAnimationLength;
    private bool isFiring = false;
    private int playerMask;
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

        if (distance < botRange) {
            time += Time.deltaTime;
            if (time > (botRate * Random.Range(0.75f, 1.40f)) && !isFiring) {
                StartCoroutine(fireBullet(10 - botAccuracy, botDamage));
            }
        }
    }

    private System.Collections.IEnumerator fireBullet(float accuracy, int damage) {
        isFiring = true;
        if (!runAndGun) {
            enemyMovement.enabled = false;
        }
        animationManager.playAttackAnimation();
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
            bullet.tag = "Enemy Projectile";
            bullet.name = $"{gameObject.name}'s {bulletPrefab.name}";
            BulletBehavior bulletScript = bullet.GetComponent<BulletBehavior>();
            bulletScript.damage = damage;

            // Fire the bullet and apply accuracy
            Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
            rotationVector.x += ((Random.value * 2) - 1) * accuracy;
            rotationVector.y += ((Random.value * 2) - 1) * accuracy;
            rotationVector.z += ((Random.value * 2) - 1) * accuracy;
            bullet.transform.rotation = Quaternion.Euler(rotationVector);
            bullet.SetActive(true);
            Destroy(bullet, bulletScript.bulletLifetime);
        } else {  // Do a melee attack using the projectile spawn point as the epicenter
            if (Physics.CheckSphere(projectileSpawnPoint.position, botRange, playerMask)) {
                playerHealth.takeDamage(damage);
            }
        }

        if (!runAndGun) {
            yield return new WaitForSeconds(attackAnimationLength - attackAnimationDelay);
            enemyMovement.enabled = true;
        }

        isFiring = false;
        time = 0;
        yield return null;
    }
}
