using UnityEngine;
using System.Collections;

public class BulletBehavior : MonoBehaviour
{
    public int damage = 10;
    public int bulletSpeed = 80;
    public Transform target;
    public Rigidbody rb;
    public PlayerShoot player;

    [Tooltip("Whether to pool projectile objects instead of instantiating.")]
    public bool poolProjectiles;
    [Tooltip("Maximum pool size. Correlates directly to fire rate: high fire rate weapon should have a large pool size.")]
    public int poolSize;

    public void Start() {
        rb.velocity = gameObject.transform.forward * bulletSpeed;
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Bullet") {  // doesn't help missiles!
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
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
        yield return null;
    }
}
