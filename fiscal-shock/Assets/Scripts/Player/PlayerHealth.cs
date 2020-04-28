using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Takes care of damage taken by the player and manages misc functions
/// like invincibility and the player flashlight.
/// </summary>
public class PlayerHealth : MonoBehaviour {
    private GameObject hitVignette;
    private float timeMultiplier = 0.01f;
    public bool invincible;
    private Light playerFlashlight;

    private void Start() {
        playerFlashlight = GameObject.FindGameObjectWithTag("Player Flashlight").GetComponent<Light>();
        resetVignette();
    }

    public void resetVignette(){
        hitVignette = GameObject.FindGameObjectWithTag("Player Hit Vignette");
        hitVignette.SetActive(false);
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Enemy Projectile") {
            BulletBehavior bullet = col.gameObject.GetComponent<BulletBehavior>();
            if (bullet.hitSomething) {
                return;
            }
            bullet.hitSomething = true;
            takeDamage(bullet.damage);
        }
    }

    /// <summary>
    /// When called, enables the HUD item to show damage for the passed amount of time.
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator showHitVignette(float duration) {
        hitVignette.SetActive(true);
        yield return new WaitForSeconds(duration);
        hitVignette.SetActive(false);

        yield return null;
    }

    /// <summary>
    /// Activates when the player is hit, if not dead the player loses money
    /// equal to the damage taken. If the player is out of money, they lose.
    /// </summary>
    public void takeDamage(float damage) {
        if (!invincible && !StateManager.playerDead) {
            StateManager.cashOnHand -= damage;
            StartCoroutine(showHitVignette(damage * timeMultiplier));
        }
        if (StateManager.cashOnHand < 0) {
            hitVignette.SetActive(false);
            StateManager.playerDead = true;
            Destroy(GameObject.Find("DungeonMusic"));
            GameObject.FindGameObjectWithTag("Loading Screen").GetComponent<LoadingScreen>().startLoadingScreen("LoseGame");
        }
    }

    /// <summary>
    /// Shows the player flashlight within the dungeons.
    /// </summary>
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

    /// <summary>
    /// Used to disable invincibility after players have a few seconds to position
    /// or if they are currently attacking themselves.
    /// </summary>
    private void Update() {
        // Disable invincibility:
        // - after 2 seconds when any key is pressed (you get 5 seconds to reposition)
        // - when LMB is pressed and time is flowing (assumed to be firing at enemy)
        if (invincible && ((Input.anyKey && Time.timeSinceLevelLoad > 5f) || (Input.GetMouseButton(0) && Time.timeSinceLevelLoad > 0.1f))) {
            invincible = false;
            playerFlashlight.color = Color.white;
        }
        if (hitVignette == null) {
            resetVignette();
        }
    }
}
