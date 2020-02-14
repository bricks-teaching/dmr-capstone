using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is to allow main camera movement in the Weapons Demo scene. It can be deleted once this demo scene is deleted.
public class WeaponDemo : MonoBehaviour
{
    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        Vector3 playerDirection = (player.transform.position - gameObject.transform.position).normalized;
        gameObject.transform.rotation = Quaternion.LookRotation(playerDirection);
        const float FlySpeed = 12.0f;
        if (Input.GetKey("right")){
            gameObject.transform.position += new Vector3(FlySpeed, 0, 0);
        }
        if (Input.GetKey("left")){
            gameObject.transform.position += new Vector3(-FlySpeed, 0, 0);
        }
        if (Input.GetKey("up")){
            gameObject.transform.position += new Vector3(0, FlySpeed, 0);
        }
        if (Input.GetKey("down")){
            gameObject.transform.position += new Vector3(0, -FlySpeed, 0);
        }
        if (Input.GetKey("w")){
            gameObject.transform.position += new Vector3(0, 0, FlySpeed);
        }
        if (Input.GetKey("s")){
            gameObject.transform.position += new Vector3(0, 0, -FlySpeed);
        }
    }
}
