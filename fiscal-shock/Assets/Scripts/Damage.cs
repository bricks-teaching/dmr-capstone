using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script controls the health of enemy bots
public class Damage : MonoBehaviour
{
    public int totalHealth = 30;
    public int startingHealth = 30;
    public GameObject explosion;
    private AudioSource hitSound;
    public AudioClip hitSoundClip;
    public GameObject controller;
    public float volume = 1f;
    // Start is called before the first frame update
    void Start()
    {
        hitSound = GetComponent<AudioSource>();
    }
    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Bullet")
        {
            //Reduce health
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            int bulletDamage = bullet.damage;
            totalHealth -= bulletDamage;
            if(totalHealth <= 0){
                WeaponDemo mainScript = controller.GetComponent(typeof(WeaponDemo)) as WeaponDemo;
                mainScript.removeBot(gameObject);
                Destroy(gameObject, 0.9f);
            }
            Debug.Log("Damage: " + bullet.damage + " points. Bot has " + totalHealth + " health points remaining");
            //Play sound effect and explosion particle system
            hitSound.PlayOneShot(hitSoundClip, 0.5f * volume);
            GameObject explode = Instantiate(explosion, gameObject.transform.position, gameObject.transform.rotation);
            Destroy(explode, 0.5f);
            //If bot goes under 50% health, make it look damaged
            if(totalHealth <= startingHealth / 2 && (totalHealth + bulletDamage) > startingHealth / 2){
                if(gameObject.tag == "RobotBug")
                {
                    for(int i = 3; i < 5; i++){
                        Vector3 randomDirection = (new Vector3(Random.value, Random.value, Random.value)).normalized;
                        gameObject.transform.GetChild(0).gameObject.
                        transform.GetChild(0).gameObject.transform.GetChild(i)
                        .gameObject.transform.rotation = Quaternion.LookRotation(randomDirection);
                    }
                } else if(gameObject.tag == "SpidBot")
                {
                    Vector3 randomDirection = (new Vector3(Random.value, Random.value, Random.value)).normalized;
                    gameObject.transform.GetChild(0).gameObject.
                    transform.GetChild(1).gameObject.transform.GetChild(0)
                    .gameObject.transform.GetChild(0).gameObject.transform
                    .rotation = Quaternion.LookRotation(randomDirection);
                    Destroy(gameObject.transform.GetChild(0).gameObject.
                    transform.GetChild(0).gameObject.transform.GetChild(0)
                    .gameObject.transform.GetChild(0).gameObject.transform.GetChild(0)
                    .gameObject.transform.GetChild(0).gameObject.transform.GetChild(0)
                    .gameObject);
                }
            }
        }
    }
}
