using UnityEngine;

// TODO: Change transformation values to floats.

//This script allows enemy bots to fire weapons.
public class EnemyShoot : MonoBehaviour {
    public GameObject bulletPrefab;
    public GameObject player;
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    private float time = 0.0f;
    public float botSize = 2.0f;
    //The ammout of damage the bot does with each shot.
    public int botDamage = 10;
    //how accurately the bot will shoot.
    public int botAccuracy = 10;
    //How close the bot needs to be to fire bullets at the player.
    public float botRange = 10f;
    // the number of seconds between shots.
    public float botRate = 1.7f;
    public float volume = 1f;
    public float gunHeight = 0;
    public EnemyMovement enemyMovement;

    void Start() {
        fireSound = GetComponent<AudioSource>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    void Update() {
        if (player == null) { return; }

        float distance = enemyMovement.getDistanceFromPlayer();

        if (distance < botRange) {
            time += Time.deltaTime;
            if (time > botRate) {
                fireBullet(10 - botAccuracy, botDamage);
                time = 0.0f;
            }
        }
    }

    void fireBullet(float accuracy, int damage) {
        fireSound.PlayOneShot(fireSoundClip, volume);
        GameObject bullet = Instantiate(bulletPrefab, gameObject.transform.position + (gameObject.transform.forward * botSize) + (gameObject.transform.up * gunHeight), gameObject.transform.rotation);
        BulletBehavior bulletScript = bullet.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
        bulletScript.damage = damage;
        Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
        rotationVector.x += ((Random.value * 2) - 1) * accuracy;
        rotationVector.y += ((Random.value * 2) - 1) * accuracy;
        rotationVector.z += ((Random.value * 2) - 1) * accuracy;
        if (gameObject.tag == "Lobber") {
            rotationVector.y += 5;
        }
        bullet.transform.rotation = Quaternion.Euler(rotationVector);
    }
}
