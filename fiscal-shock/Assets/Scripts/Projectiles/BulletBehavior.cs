using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public int damage = 10;
    public int bulletSpeed = 80;

    void Start() {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = gameObject.transform.forward * bulletSpeed;
    }

    void OnCollisionEnter()
    {
        Destroy(gameObject);
    }
}
