using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public int damage = 10;
    public int bulletSpeed = 80;
    public Transform target;
    public Rigidbody rb;
    public PlayerShoot player;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.velocity = gameObject.transform.forward * bulletSpeed;
    }

    void OnCollisionEnter()
    {
        Destroy(gameObject);
    }

    void OnDestroy(){
        if(player != null){
            player.removeMissle(gameObject);
        }
    }

    
}
