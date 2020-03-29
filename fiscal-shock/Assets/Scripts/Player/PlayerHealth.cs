using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Bullet") {
            //Debug.Log("Player hit");
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            takeDamage(bullet.damage);
        }
    }

    public void takeDamage(float damage) {
        PlayerFinance.cashOnHand -= damage;
    }
}
