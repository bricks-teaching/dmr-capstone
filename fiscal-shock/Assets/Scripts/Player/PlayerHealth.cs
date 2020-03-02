using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Bullet") {
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            PlayerFinance.cashOnHand -= bullet.damage;
        }
    }
}
