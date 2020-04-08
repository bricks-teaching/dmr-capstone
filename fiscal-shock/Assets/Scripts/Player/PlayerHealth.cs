using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {
    private GameObject hitVignette;
    private float timeMultiplier = 0.01f;
    public bool invincible;
    private Light playerFlashlight;

    private void Start() {
        playerFlashlight = GameObject.FindGameObjectWithTag("Player Flashlight").GetComponent<Light>();
        if (hitVignette == null) {
            hitVignette = GameObject.FindGameObjectWithTag("Player Hit Vignette");
            hitVignette.SetActive(false);
        }
    }

    public void resetVignette(){
        if (hitVignette == null) {
            hitVignette = GameObject.FindGameObjectWithTag("Player Hit Vignette");
            hitVignette.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Enemy Projectile") {
            BulletBehavior bullet = col.gameObject.GetComponent<BulletBehavior>();
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
        if (!invincible) {
            StateManager.cashOnHand -= damage;
            StartCoroutine(showHitVignette(damage * timeMultiplier));
        }
        if (StateManager.cashOnHand < 0) {
            GameObject.FindGameObjectWithTag("Loading Screen").GetComponent<LoadingScreen>().startLoadingScreen("LoseGame");
        }
    }

    public IEnumerator enableIframes(float duration) {
        if (playerFlashlight == null) {  // if start in dungeon scene
            playerFlashlight = GameObject.FindGameObjectWithTag("Player Flashlight").GetComponent<Light>();
        }
        invincible = true;
        playerFlashlight.color = new Color(0, 1, 0.75f, 0.5f);
        yield return new WaitForFixedUpdate();  // そして時は動き出す…
        yield return new WaitForSeconds(duration);
        invincible = false;
        playerFlashlight.color = Color.white;
        yield return null;
    }

    private void Update() {
        // Disable invincibility:
        // - after 2 seconds when any key is pressed (you get 2 seconds to reposition)
        // - when LMB is pressed and time is flowing (assumed to be firing at enemy)
        if (invincible && ((Input.anyKey && Time.timeSinceLevelLoad > 2f) || (Input.GetMouseButton(0) && Time.timeSinceLevelLoad > 0.1f))) {
            invincible = false;
            playerFlashlight.color = Color.white;
        }
    }
}
