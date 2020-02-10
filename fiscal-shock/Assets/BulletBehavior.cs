using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public Rigidbody rb;
    private float time = 0.0f;
    public int damage = 10;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //gameObject.transform.Rotate(new Vector3(-90, 0, 0), Space.World);
        Fire();
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "RobotBug")
        {
            Debug.Log("Hit!");

            Destroy(gameObject);
        }/* else if(col.gameObject.tag == "MainCamera")
        {
            Debug.Log("Player Hit!");

            Destroy(gameObject);
        }
        */
    }

    void Fire()
    {
        Debug.Log("Fire!!");
        rb.velocity = gameObject.transform.forward * 1000;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rb.velocity = new Vector3(0, 0, 20);
        time += Time.fixedDeltaTime;
        if( time > 1.0f){
            Debug.Log("20!");
            Destroy(gameObject);
        }
    }
}
