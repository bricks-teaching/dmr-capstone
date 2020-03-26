using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {

    public Image hitVinette;
    private float time = 0.1f;

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Bullet") {
            //Debug.Log("Player hit");
            BulletBehavior bullet = col.gameObject.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
            PlayerFinance.cashOnHand -= bullet.damage;
            hitVinette.enabled = true;
            Time.timeScale = 0;
            if(hitVinette.enabled == true){
                StartCoroutine(removeHitDelay());
            }
            Time.timeScale = 1;
        }
    }

    private IEnumerator<WaitForSeconds> removeHitDelay() {
        yield return new WaitForSeconds(time);

        hitVinette.enabled = false;
        while (hitVinette.enabled == true) {
            yield return null;
        }
    }
}
