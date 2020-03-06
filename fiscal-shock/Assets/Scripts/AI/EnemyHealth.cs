using UnityEngine;

//This script controls the health of enemy bots
public class EnemyHealth: MonoBehaviour
{
    public int startingHealth = 30;
    public GameObject explosion;
    public GameObject bigExplosion;
    public AudioClip hitSoundClip;
    public float pointValue = 20;
    private int totalHealth;
    private GameObject lastBulletCollision;
    public AnimationManager animationManager;
    private bool dead;

    void Start(){
        totalHealth = startingHealth;
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Bullet" || col.gameObject.tag == "Missle") {
            if (col.gameObject == lastBulletCollision){
                return;
            }
            lastBulletCollision = col.gameObject;

            // Reduce health
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            int bulletDamage = bullet.damage;
            totalHealth -= bulletDamage;
            if (totalHealth <= 0 && !dead) {
                PlayerFinance.cashOnHand += pointValue;
                float deathDuration = animationManager.playDeathAnimation();
                GetComponent<EnemyMovement>().enabled = false;
                GetComponent<EnemyShoot>().enabled = false;
                animationManager.animator.PlayQueued("shrink");
                Destroy(gameObject, deathDuration + 0.5f);
                dead = true;
            }

            // Debug.Log("Damage: " + bullet.damage + " points. Bot has " + totalHealth + " health points remaining");
            // Play sound effect and explosion particle system
            GameObject explode = null;
            if(col.gameObject.tag == "Bullet"){
                explode = Instantiate(explosion, gameObject.transform.position + transform.up, gameObject.transform.rotation);
                AudioSource hitSound = explode.GetComponent<AudioSource>();
                hitSound.PlayOneShot(hitSoundClip, 0.4f * Settings.volume);
            } else if(col.gameObject.tag == "Missle"){
                explode = Instantiate(bigExplosion, gameObject.transform.position + transform.up, gameObject.transform.rotation);
                AudioSource hitSound = explode.GetComponent<AudioSource>();
                hitSound.PlayOneShot(hitSoundClip, 0.65f * Settings.volume);
            }
            explode.transform.parent = gameObject.transform;
            Destroy(explode, 0.9f);

            // If bot goes under 50% health, make it look damaged
            if (totalHealth <= startingHealth / 2 && (totalHealth + bulletDamage) > startingHealth / 2) {
                if (gameObject.tag == "Blaster") {
                    for (int i = 0; i < 2; i++) {
                        Vector3 randomDirection = new Vector3(Random.value, Random.value, Random.value).normalized;
                        gameObject.transform.GetChild(0).gameObject.
                        transform.GetChild(0).gameObject.
                        transform.GetChild(2).gameObject.transform.GetChild(i)
                        .gameObject.transform.GetChild(0).gameObject.transform.rotation = Quaternion.LookRotation(randomDirection);
                    }
                }
                if (gameObject.tag == "Lobber") {
                    for (int i = 0; i < 2; i++) {
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
