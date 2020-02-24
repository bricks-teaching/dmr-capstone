using UnityEngine;

public class playerDamage : MonoBehaviour
{
    public int health = 1000;

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Bullet")
        {
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            health -= bullet.damage;
            if(health <= 0){
                Time.timeScale = 0;
                Debug.Log("You lost");
            }
            Debug.Log("Health remaining: " + health);
        }
    }
}
