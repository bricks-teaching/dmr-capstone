using UnityEngine;
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

    [Tooltip("Whether to pool projectile objects instead of instantiating.")]
    public bool poolProjectiles;

    [Tooltip("Maximum pool size. Correlates directly to fire rate: a high fire rate weapon should have a large pool size.")]
    public int poolSize;

    /* Variables set during runtime */
    public Transform target { get; set; }
    public PlayerShoot player { get; set; }

    public void Start() {
        rb.velocity = gameObject.transform.forward * bulletSpeed;
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Bullet" | col.gameObject.layer == LayerMask.NameToLayer("Player")) {  // doesn't help missiles!
            return;
        }
        if (poolProjectiles) {
            gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
        }
    }

    void OnDestroy() {
        player?.removeMissile(gameObject);
    }

    public IEnumerator timeout() {
        yield return new WaitForSeconds(bulletLifetime);
        gameObject.SetActive(false);
        yield return null;
    }
}
