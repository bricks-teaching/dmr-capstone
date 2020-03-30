using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {

    public Image hitVignette;
    private float time = 0.1f;

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Bullet") {
            //Debug.Log("Player hit");
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            PlayerFinance.cashOnHand -= bullet.damage;
            hitVignette.enabled = true;
            Time.timeScale = 0;
            if(hitVignette.enabled == true){
                StartCoroutine(removeHitDelay());
            }
            Time.timeScale = 1;
        }
    }

    private IEnumerator<WaitForSeconds> removeHitDelay() {
        yield return new WaitForSeconds(time);

        hitVignette.enabled = false;
        while (hitVignette.enabled == true) {
            yield return null;
        }
    }
}
