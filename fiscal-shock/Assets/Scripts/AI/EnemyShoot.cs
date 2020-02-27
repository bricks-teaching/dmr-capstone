using UnityEngine;

//This script allows enemy bots to fire weapons.
public class EnemyShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject player;
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    private float time = 0.0f;
    public int botSize = 2;
    //The ammout of damage the bot does with each shot.
    public int botDamage = 10;
    //how accurately the bot will shoot.
    public int botAccuracy = 10;
    //How close the bot needs to be to fire bullets at the player.
    public int botRange = 1440;
    //How close the bot gets to the player
    public int botStoppingDistance = 1000;
    // the number of seconds between shots.
    public float botRate = 1.7f;
    public float botReaction = 7.0f;
    public float botSpeed = 20f;
    public float volume = 1f;
    public float gunHeight = 0;

    void Start()
    {
        fireSound = GetComponent<AudioSource>();
    }

    public void Update()
    {
        if(player == null){return;}
        Vector3 playerDirection = (player.transform.position - gameObject.transform.position).normalized;
        if(gameObject.tag == "Lobber"){
            playerDirection.y = 0;
       }
        Quaternion rotatationToPlayer = Quaternion.LookRotation(playerDirection);
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, rotatationToPlayer, Time.fixedDeltaTime * botReaction);
        //Debug.Log("Distance: " + (gameObject.transform.position - player.transform.position).magnitude);
        float distance = (gameObject.transform.position - player.transform.position).magnitude;
        if( distance < botRange)
        {
            time += Time.deltaTime;
            if(time > botRate){
                fireBullet(10 - botAccuracy, botDamage);
                time = 0.0f;
            }
        }
        if( distance > botStoppingDistance)
        { // This will be replaced by AI pathfinding later
            gameObject.transform.position += playerDirection * botSpeed * Time.deltaTime; //(gameObject.transform.forward * botSpeed);
        }
    }

    void fireBullet(float accuracy, int damage)
    {
        fireSound.PlayOneShot(fireSoundClip, volume);
        GameObject bullet = Instantiate(bulletPrefab, gameObject.transform.position + (gameObject.transform.forward * botSize) + (gameObject.transform.up * gunHeight), gameObject.transform.rotation);
        BulletBehavior bulletScript = bullet.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
        bulletScript.damage = damage;
        Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
        rotationVector.x += ((Random.value * 2) - 1) * accuracy;
        rotationVector.y += ((Random.value * 2) - 1) * accuracy;
        rotationVector.z += ((Random.value * 2) - 1) * accuracy;
        if(gameObject.tag == "Lobber"){
            rotationVector.y += 5;
        }
        bullet.transform.rotation = Quaternion.Euler(rotationVector);
    }
}
