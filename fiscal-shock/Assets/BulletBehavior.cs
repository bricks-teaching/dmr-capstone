using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    private float time = 0.0f;
    public int damage = 10;
    public int bulletSpeed = 1800;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = gameObject.transform.forward * bulletSpeed;
    }

    void OnCollisionEnter(Collision col)
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //If the bullet has not hit anything after one second it is removed from the scene
        time += Time.fixedDeltaTime;
        if( time > 1.0f){
            Destroy(gameObject);
        }
    }
}
