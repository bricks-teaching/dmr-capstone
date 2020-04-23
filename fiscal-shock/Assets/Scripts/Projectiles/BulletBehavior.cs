using UnityEngine;
using System.Collections;

public class BulletBehavior : MonoBehaviour
{
    [Tooltip("Amount of damage this bullet deals when fired by the player.")]
    public float damage = 10;

    [Tooltip("How fast this bullet travels.")]
    public float bulletSpeed = 80;

    [Tooltip("Reference to this bullet's rigidbody component.")]
    public Rigidbody rb;

    [Tooltip("How long the bullet persists after being fired. Longer lifetimes are required for bullets that should travel far.")]
    public float bulletLifetime = 2f;

    [Tooltip("Maximum pool size. Correlates directly to fire rate: a high fire rate weapon should have a large pool size.")]
    public int poolSize = 1;

    /* Variables set during runtime */
    /// <summary>
    /// Body of the target that was hit by the raycast.
    /// </summary>
    /// <value></value>
    public Transform target { get; set; }
    /// <summary>
    /// Exact point on the body where the target was hit by the raycast.
    /// </summary>
    /// <value></value>
    public Vector3 localizedTarget { get; set; }
    /// <summary>
    /// Whether this bullet has hit something.
    /// </summary>
    /// <value></value>
    public bool hitSomething { get; set; }
    /// <summary>
    /// Direction of ricochet for bullets that don't expire on hit.
    /// </summary>
    private Vector3 ricochetDirection;
    /// <summary>
    /// How long ago did this bullet (if homing) update its trajectory?
    /// Used for enemy homing missiles.
    /// </summary>
    private float timeSinceLastAiming;
    /// <summary>
    /// Current hard-coded value for how often enemy homing missiles
    /// update. Lower update rates make it really hard to dodge them.
    /// </summary>
    private float homingUpdateRate = 0.8f;
    /// <summary>
    /// Whether this bullet has a real, live target it's seeking.
    /// </summary>
    /// <value></value>
    public bool seekingTarget { get; set; }
    /// <summary>
    /// Whether this bullet can ricochet. Should disable for automatic weapons.
    /// </summary>
    /// <value></value>
    public bool ricochetEnabled { get; set; }

    /// <summary>
    /// Set the bullet's trajectory when it's enabled (fired).
    /// </summary>
    public void OnEnable() {
        rb.velocity = transform.forward * bulletSpeed;
    }

    /// <summary>
    /// Clean up current behavior and references when the bullet is disabled.
    /// Disabling implies a return to the object pool, or beginning initial
    /// setup of the bullet before firing.
    /// </summary>
    public void OnDisable() {
        StopAllCoroutines();  // disable timeout if it's happening
        target = null;
        localizedTarget = Vector3.zero;
        hitSomething = false;
        rb.velocity = Vector3.zero;
        seekingTarget = false;
    }

    /// <summary>
    /// Behavior when the bullet collides with something.
    /// </summary>
    /// <param name="col"></param>
    void OnCollisionEnter(Collision col) {
        if (gameObject.tag != "Enemy Projectile" && (col.gameObject.tag == "Bullet" || col.gameObject.layer == LayerMask.NameToLayer("Player"))) {
            // Do nothing when these objects are hit
            return;
        }
        if (gameObject.tag == "Enemy Projectile") {
            if (col.gameObject.tag == "Bullet" || col.gameObject.tag == "Missile") {  // Allow player to disable enemy projectiles
                hitSomething = true;
            } else {
                // If an enemy bullet has struck something, delete it (no ricochet)
                Destroy(gameObject);
            }
        }
        if (ricochetEnabled && !hitSomething) {  // ricochet just once
            hitSomething = true;
            // Apply ricochet
            Vector3 norm = col.GetContact(0).normal;
            ricochetDirection = Vector3.Reflect(transform.position, norm * 20).normalized;
            target = null;
        }
    }

    /// <summary>
    /// Disable the bullet after its lifetime expires. Must be called by the
    /// firing script explicitly. Only used by the player, as enemy bullets are
    /// not currently pooled.
    /// </summary>
    /// <returns></returns>
    public IEnumerator timeout() {
        yield return new WaitForSeconds(bulletLifetime);
        transform.gameObject.SetActive(false);
        yield return null;
    }

    /// <summary>
    /// Physics updates for the bullet.
    /// </summary>
    void FixedUpdate() {
        // Enemy projectiles don't update their trajectory every update
        if (gameObject.tag == "Enemy Projectile" && target != null) {
            timeSinceLastAiming += Time.deltaTime;
            if (timeSinceLastAiming >= homingUpdateRate) {
                timeSinceLastAiming = 0;
            } else {
                return;
            }
        }
        // Homing missiles that haven't struck something should continue seeking
        if (target != null && !hitSomething) {
            rb.velocity = (target.TransformPoint(localizedTarget) - transform.position).normalized * bulletSpeed;
            transform.LookAt(target);
            // After the initial targeting, disable homing
            if (!seekingTarget) {
                target = null;
            }
        }
        // Apply ricochet to bullets that have struck something
        if (hitSomething) {
            rb.velocity = ricochetDirection * bulletSpeed;
        }
    }
}
