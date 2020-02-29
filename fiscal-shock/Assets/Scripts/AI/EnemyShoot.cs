using UnityEngine;

//This script allows enemy bots to fire weapons.
public class EnemyShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject player;
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    private float time = 0.0f;
    public int botSize = 2;
    //The ammout of damage the bot does with each shot.
    public int botDamage = 10;
    //how accurately the bot will shoot.
    public int botAccuracy = 10;
    //How close the bot needs to be to fire bullets at the player.
    public int botRange = 15;
    //How close the bot gets to the player
    public int botStoppingDistance = 3;
    // the number of seconds between shots.
    public float botRate = 2.7f;
    public float botReaction = 3.5f;
    public float botSpeed = 4f;
    public float volume = 1f;
    public float gunHeight = 0;
    public float visionRadius = 35f;  // How far bot can see

    void Start()
    {
        fireSound = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void Update()
    {
        if (player == null) {
            return;
        }
        if (Vector3.Distance(player.transform.position, gameObject.transform.position) > visionRadius) {
            // TODO idle animation instead
            return;
        }

        Vector3 playerDirection = (player.transform.position - gameObject.transform.position).normalized;

        if (gameObject.tag == "Lobber") {
            playerDirection.y = 0;
        }

        // Turn to face the player
        Quaternion rotatationToPlayer = Quaternion.LookRotation(playerDirection);
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, rotatationToPlayer, Time.fixedDeltaTime * botReaction);

        // See if player is close enough to fire at
        float distance = (gameObject.transform.position - player.transform.position).magnitude;
        if (distance < botRange) {
            time += Time.deltaTime;
            if(time > botRate) {
                fireBullet(10 - botAccuracy, botDamage);
                time = 0.0f;
            }
        }

        // See if I want to move closer to the player
        if (distance > botStoppingDistance) { // This will be replaced by AI pathfinding later
            gameObject.transform.position += playerDirection * botSpeed * Time.deltaTime; //(gameObject.transform.forward * botSpeed);
        }
    }

    void fireBullet(float accuracy, int damage) {
        // Start sound effect
        fireSound.PlayOneShot(fireSoundClip, volume);

        // Instantiate the projectile
        GameObject bullet = Instantiate(bulletPrefab, gameObject.transform.position + (gameObject.transform.forward * botSize) + (gameObject.transform.up * gunHeight), gameObject.transform.rotation);
        BulletBehavior bulletScript = bullet.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
        bulletScript.damage = damage;

        // Fire the bullet
        Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
        rotationVector.x += ((Random.value * 2) - 1) * accuracy;
        rotationVector.y += ((Random.value * 2) - 1) * accuracy;
        rotationVector.z += ((Random.value * 2) - 1) * accuracy;

        // Adjust rotation for the shorty, should do this smarter
        if (gameObject.tag == "Lobber"){
            rotationVector.y += 5;
        }
        bullet.transform.rotation = Quaternion.Euler(rotationVector);
        Destroy(bullet, 1f);
    }
}
