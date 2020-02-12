using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script allows enemy bots to fire weapons.
public class Shoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject player;
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    private float time = 0.0f;
    public int botSize = 90;
    //The ammout of damage the bot does with each shot.
    public int botDamage = 10;
    //how accurately the bot will shoot.
    public int botAccuracy = 10;
    //How close the bot needs to be to fire bullets at the player.
    public int botRange = 1440;
    // the number of seconds between shots.
    public float botRate = 1.7f;
    public float botReaction = 7.0f;
    public float botSpeed = 20f;
    // Start is called before the first frame update
    void Start()
    {
        fireSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerDirection = (player.transform.position - gameObject.transform.position).normalized;
        Quaternion rotatationToPlayer = Quaternion.LookRotation(playerDirection);
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, rotatationToPlayer, Time.fixedDeltaTime * botReaction);
        //Debug.Log("Distance: " + (gameObject.transform.position - player.transform.position).magnitude);
        if((gameObject.transform.position - player.transform.position).magnitude < botRange)
        {
            time += Time.deltaTime;
            if(time > botRate){
                fireBullet(botAccuracy, botDamage);
                time = 0.0f;
            }
        } else { // This will be replaced by AI pathfinding later
            gameObject.transform.position += gameObject.transform.forward * botSpeed;
        }
    }

    void fireBullet(float accuracy, int damage)
    {
        fireSound.PlayOneShot(fireSoundClip);
        GameObject bullet = Instantiate(bulletPrefab, gameObject.transform.position + (gameObject.transform.forward * botSize), gameObject.transform.rotation) as GameObject;
        BulletBehavior bulletScript = (bullet.GetComponent(typeof(BulletBehavior)) as BulletBehavior);
        bulletScript.damage = damage;
        Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
        rotationVector.x += ((Random.value * 2) - 1) * accuracy;
        rotationVector.y += ((Random.value * 2) - 1) * accuracy;
        rotationVector.z += ((Random.value * 2) - 1) * accuracy;
        bullet.transform.rotation = Quaternion.Euler(rotationVector);
    }
}
