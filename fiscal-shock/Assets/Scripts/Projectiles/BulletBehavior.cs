using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    private float time = 0.0f;
    public int damage = 10;
    public int bulletSpeed = 80;
    // Start is called before the first frame update
    void Start()
    {
        //Rigidbody rb = GetComponent<Rigidbody>();
        //rb.velocity = gameObject.transform.forward * bulletSpeed;
    }

    void OnCollisionEnter()
    {
        Destroy(gameObject);
    }
}
