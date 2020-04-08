﻿using UnityEngine;

// TODO: Change transformation values to floats.

//This script allows enemy bots to fire weapons.
public class EnemyShoot : MonoBehaviour {
    public GameObject bulletPrefab;
    public GameObject player { get; set; }
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    private float time = 0.0f;
    public float botSize = 2.0f;

    [Tooltip("Amount of damage done per shot.")]
    public int botDamage = 10;

    [Tooltip("How accurately the bot fires.")]
    public int botAccuracy = 10;

    [Tooltip("How close the bot must be to begin firing.")]
    public float botRange = 6f;

    [Tooltip("Fire rate of the bot.")]
    public float botRate = 1.7f;

    public float gunHeight = 0;
    public EnemyMovement enemyMovement;
    public bool spottedPlayer;
    public AnimationManager animationManager;

    void Start() {
        fireSound = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update() {
        if (player == null || !spottedPlayer) { return; }

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
        fireSound.PlayOneShot(fireSoundClip, Settings.volume);
        animationManager.playAttackAnimation();

        // Instantiate the projectile
        // Assumes bot is facing the player, so fire in that direction
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position + (transform.forward * botSize) + (transform.up * gunHeight),
            transform.rotation);
        bullet.SetActive(false);
        bullet.transform.parent = transform;
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
    }
}
