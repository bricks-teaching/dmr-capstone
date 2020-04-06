﻿using UnityEngine;
using System.Collections;

public class BulletBehavior : MonoBehaviour
{
    [Tooltip("Amount of damage this bullet deals when fired by the player.")]
    public int damage = 10;

    [Tooltip("How fast this bullet travels.")]
    public int bulletSpeed = 80;

    [Tooltip("Reference to this bullet's rigidbody component.")]
    public Rigidbody rb;

    [Tooltip("How long the bullet persists after being fired. Longer lifetimes are required for bullets that should travel far.")]
    public float bulletLifetime = 2f;

    [Tooltip("Maximum pool size. Correlates directly to fire rate: a high fire rate weapon should have a large pool size.")]
    public int poolSize = 1;

    /* Variables set during runtime */
    public Transform target { get; set; }
    public Vector3 localizedTarget { get; set; }
    public PlayerShoot player { get; set; }
    public bool hitSomething { get; private set; }
    public bool grounded { get; private set; }

    public void OnEnable() {
        rb.velocity = transform.forward * bulletSpeed;
    }

    public void OnDisable() {
        StopAllCoroutines();  // disable timeout if it's happening
        target = null;
        localizedTarget = Vector3.zero;
        hitSomething = false;
        grounded = false;
        rb.velocity = Vector3.zero;
    }

    void OnCollisionEnter(Collision col) {
        hitSomething = true;
        if (col.gameObject.tag == "Bullet" || col.gameObject.layer == LayerMask.NameToLayer("Player")) {  // doesn't help missiles!
            return;
        } else if (col.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            grounded = true;
        }
        if (gameObject.tag != "Enemy Projectile") {  // those aren't pooled... yet
            transform.gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
        }
    }

    public IEnumerator timeout() {
        yield return new WaitForSeconds(bulletLifetime);
        transform.gameObject.SetActive(false);
        yield return null;
    }

    void Update() {
        if (target != null && !hitSomething) {
            rb.velocity = (target.TransformPoint(localizedTarget) - transform.position).normalized * bulletSpeed;
        }
    }
}