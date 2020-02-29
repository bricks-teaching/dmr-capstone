using UnityEngine;

//This script controls the health of enemy bots
public class EnemyHealth: MonoBehaviour
{
    public int startingHealth = 30;
    public GameObject explosion;
    public AudioClip hitSoundClip;
    public float pointValue = 20;
    public float volume = 1f;
    private int totalHealth;
    private GameObject lastBulletCollision;

    void Start(){
        totalHealth = startingHealth;
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Bullet")
        {
            if(col.gameObject == lastBulletCollision){return;}
            lastBulletCollision = col.gameObject;
            //Reduce health
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            int bulletDamage = bullet.damage;
            totalHealth -= bulletDamage;
            if(totalHealth <= 0){
                GameController.removeBot(gameObject);
                PlayerFinance.cashOnHand += pointValue;
                Destroy(gameObject);
            }
            Debug.Log("Damage: " + bullet.damage + " points. Bot has " + totalHealth + " health points remaining");
            GameObject explode = Instantiate(explosion, gameObject.transform.position + transform.up, gameObject.transform.rotation);
            //Play sound effect and explosion particle system
            AudioSource hitSound = explode.GetComponent<AudioSource>();
            hitSound.PlayOneShot(hitSoundClip, 0.5f * volume);
            Destroy(explode, 0.9f);
            //If bot goes under 50% health, make it look damaged
            if(totalHealth <= startingHealth / 2 && (totalHealth + bulletDamage) > startingHealth / 2){
                if(gameObject.tag == "Blaster")
                {
                    for(int i = 0; i < 2; i++){
                        Vector3 randomDirection = new Vector3(Random.value, Random.value, Random.value).normalized;
                        gameObject.transform.GetChild(0).gameObject.
                        transform.GetChild(0).gameObject.
                        transform.GetChild(2).gameObject.transform.GetChild(i)
                        .gameObject.transform.GetChild(0).gameObject.transform.rotation = Quaternion.LookRotation(randomDirection);
                    }
                }
                if(gameObject.tag == "Lobber")
                {
                    for(int i = 0; i < 2; i++){
                        Debug.Log(gameObject.transform.GetChild(0).gameObject.
                        transform.GetChild(0).gameObject.
                        transform.GetChild(4).gameObject.transform.GetChild(2 * i)
                        .gameObject);
                        gameObject.transform.GetChild(0).gameObject.
                        transform.GetChild(0).gameObject.
                        transform.GetChild(4).gameObject.transform.GetChild(2 * i)
                        .gameObject.transform.position += new Vector3(0, 0.1f, 0);
                    }
                }
            }
        }
    }
}
