using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{

    public int totalHealth = 30;
    private AudioSource hitSound;
    public AudioClip hitSoundClip;
    // Start is called before the first frame update
    void Start()
    {
        hitSound = GetComponent<AudioSource>();
    }
    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Bullet")
        {
            hitSound.PlayOneShot(hitSoundClip, 0.4f);
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            Debug.Log("-" + bullet.damage + " points");
            totalHealth -= bullet.damage;
            if(totalHealth <= 0){
                Destroy(gameObject);
            }
            if(gameObject.tag == "RobotBug")
            {
                Destroy(gameObject.transform.GetChild(0).gameObject.
                transform.GetChild(0).gameObject.transform.GetChild(3)
                .gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
