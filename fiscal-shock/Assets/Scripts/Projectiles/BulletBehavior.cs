using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public int damage = 10;
    public int bulletSpeed = 80;

    void OnCollisionEnter()
    {
        Destroy(gameObject);
    }
}
