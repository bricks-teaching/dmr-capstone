using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject player;
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    public GameObject weapon;
    private float time = 0.0f;
    //The ammout of damage the bot does with each shot.
    public int botDamage = 10;
    //how accurately the bot will shoot.
    public int botAccuracy = 10;
    //How close the bot needs to be to fire bullets at the player.
    public int botRange = 1440;
    // the number of seconds between shots.
    public float botRate = 1.7f;
    public float botReaction = 7.0f;
    // Start is called before the first frame update
    void Start()
    {
        fireSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.tag == "MainCamera")
        {
            //This code is just for testing
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
            //End of testing code
            if (Input.GetMouseButtonDown(0))
            {
                WeaponStats weaponScript = (weapon.GetComponent(typeof(WeaponStats)) as WeaponStats);
                fireBullet(weaponScript.accuracy, weaponScript.strength);
            }
        } else if(gameObject.tag == "RobotBug"){
            Vector3 playerDirection = (player.transform.position - gameObject.transform.position).normalized;
            Quaternion rotatationToPlayer = Quaternion.LookRotation(playerDirection);
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, rotatationToPlayer, Time.fixedDeltaTime * botReaction);
            //Debug.Log("Distance: " + (gameObject.transform.position - player.transform.position).magnitude);
            if((gameObject.transform.position - player.transform.position).magnitude < botRange)
            {
                time += Time.fixedDeltaTime;
                if(time > botRate){
                    fireBullet(botAccuracy, botDamage);
                    time = 0.0f;
                }
            }
        }
    }

    void fireBullet(float accuracy, int damage)
    {
        fireSound.PlayOneShot(fireSoundClip);
        GameObject bullet = Instantiate(bulletPrefab, gameObject.transform.position + gameObject.transform.forward * 70, gameObject.transform.rotation) as GameObject;
        BulletBehavior bulletScript = (bullet.GetComponent(typeof(BulletBehavior)) as BulletBehavior);
        bulletScript.damage = damage;
        Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
        rotationVector.x += Random.value * 2 * accuracy - accuracy;
        rotationVector.y += Random.value * 2 * accuracy - accuracy;
        rotationVector.z += Random.value * 2 * accuracy - accuracy;
        bullet.transform.rotation = Quaternion.Euler(rotationVector);
    }
}
