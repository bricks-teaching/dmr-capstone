using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {
    private GameObject hitVignette;
    private float timeMultiplier = 0.01f;

    private void Start() {
        if (hitVignette == null) {
            hitVignette = GameObject.FindGameObjectWithTag("Player Hit Vignette");
            hitVignette.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Enemy Projectile") {
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            takeDamage(bullet.damage);
        }
    }

    private IEnumerator showHitVignette(float duration) {
        hitVignette.SetActive(true);
        yield return new WaitForSeconds(duration);
        hitVignette.SetActive(false);

        yield return null;
    }

    public void takeDamage(float damage) {
        PlayerFinance.cashOnHand -= damage;
        StartCoroutine(showHitVignette(damage * timeMultiplier));
    }
}
