using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Mutable player firing state
/// </summary>
public class PlayerFiringState {
    public bool firingAutomatic;
    public bool alreadyFired;

    public bool needToUpdateLossFeed;
    public float nextLossCost;

    public bool weaponCooling;
    public bool holsteringWeapon;
    public bool drawingWeapon;
}

public class PlayerShoot : MonoBehaviour {
    public AudioClip outOfAmmo;
    public AudioClip fireRateSound;
    public List<GameObject> guns;  // TODO inaccessible in inspector

    /* Assigned at runtime */
    public GameObject weapon { get; private set; }
    public Image crossHair { get; private set; }
    private AudioSource gunAudioSource => currentWeaponStats?.gunAudioSource;
    private WeaponStats currentWeaponStats;
    private FeedbackController feed;

    /* Related to current state */
    /// <summary>
    /// Mutable state data to be passed to weaponstats
    /// </summary>
    public PlayerFiringState state = new PlayerFiringState();

    /// <summary>
    /// Currently equipped weapon slot, equal to guns[slot-1]
    /// </summary>
    public int slot = 0;

    /// <summary>
    /// Whether the player is currently firing, kept private here for
    /// the input action callbacks
    /// </summary>
    private bool firing;
    private float animatedTime;

    public void OnFire(InputAction.CallbackContext cont) {
        if (cont.phase == InputActionPhase.Canceled || Time.timeScale == 0) {
            state.alreadyFired = false;
            firing = false;
            return;
        }
        firing = cont.phase == InputActionPhase.Performed || cont.phase == InputActionPhase.Started;
        state.alreadyFired = !firing;
    }

    public void OnWeaponSwap(InputAction.CallbackContext cont) {
        if (cont.phase != InputActionPhase.Performed || Time.timeScale == 0 || state.drawingWeapon || state.holsteringWeapon) {
            return;
        }
        slot += (int)cont.ReadValue<float>();
        if (slot >= guns.Count) {  // Wrap around after the last weapon
            slot = 0;
        } else if (slot < 0) {
            slot = guns.Count-1;
        }
    }

    public void OnWeaponSelect(InputAction.CallbackContext cont) {
        if (state.drawingWeapon || state.holsteringWeapon) {
            return;
        }
        int selectedSlot = (int)cont.ReadValue<float>();
        if (selectedSlot > 0 && selectedSlot <= guns.Count) {
            slot = selectedSlot - 1;
        }
    }

    public void Start() {
        feed = GameObject.FindGameObjectWithTag("HUD").GetComponent<FeedbackController>();
        GameObject tmp = GameObject.FindGameObjectWithTag("Crosshair");
        if (tmp != null) {
            crossHair = tmp.GetComponentInChildren<Image>(true);
            crossHair.enabled = false;
        }
        if (guns == null || guns.Count < 1) {
            guns = new List<GameObject>();
        }
        LoadWeapon();
    }

    public void resetFeed() {
        feed = GameObject.FindGameObjectWithTag("HUD").GetComponent<FeedbackController>();
    }

    void OnDisable() {
        stopFiringAuto();
        hideWeapon();
    }

    void OnEnable() {
        if (guns == null || guns.Count < 1) {
            return;
        }
        LoadWeapon();
    }

    private void hideWeapon() {
        if (weapon != null) {
            weapon.SetActive(false);
            // sometimes, it's null, strangely
            if (crossHair != null) {
                crossHair.enabled = false;
            }
        }
    }

    private void stopFiringAuto() {
        state.firingAutomatic = false;
        gunAudioSource?.Stop();
    }

    public void Update() {
        /* Check to see if we need to stop automatic firing */
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Hub" || StateManager.playerDead || StateManager.playerWon) {
            stopFiringAuto();
            return;
        }

        // Released button on auto-fire weapons
        if (state.firingAutomatic && !firing && currentWeaponStats.fireDelay < 0.03f) {
            stopFiringAuto();
            gunAudioSource.PlayOneShot(currentWeaponStats.autoStop, Settings.volume);
        }

        // Time stopped, do nothing
        if (Time.timeScale == 0) {
            return;
        }

        if (state.needToUpdateLossFeed) {
            feed?.shoot(state.nextLossCost);
            state.needToUpdateLossFeed = false;
        }

        // Start weapon swap if necessary
        HolsterWeapon();
        LoadWeapon();
        playWeaponAnimation();

        // Don't proceed to try firing until we're done changing weapons
        // If we don't have a weapon by this point, we can't continue
        if (state.drawingWeapon || state.holsteringWeapon || weapon == null) {
            return;
        }

        /**********************************************************************/

        // After this point, only continue if we're trying to fire
        if (!firing) {
            return;
        }

        // Make sure player has enough money to fire
        if (!StateManager.inStoryTutorial && StateManager.cashOnHand < currentWeaponStats.bulletCost) {
            if (!state.alreadyFired) {  // otherwise, the sound is auto fired
                stopFiringAuto();
                gunAudioSource.PlayOneShot(outOfAmmo, Settings.volume);
                state.alreadyFired = true;
            }
            return;
        }

        // Can we fire yet?
        if (state.weaponCooling) {
            if (!state.alreadyFired && !state.firingAutomatic) {  // otherwise, the sound is auto fired
                gunAudioSource.PlayOneShot(fireRateSound, Settings.volume);
                state.alreadyFired = true;
            }
            return;
        }

        // Finally, we can fire...
        currentWeaponStats.fire(state);
        StartCoroutine(firingCooldown());
    }

    /// <summary>
    /// Fade the crosshair while we're not allowed to shoot
    /// </summary>
    /// <returns></returns>
    private IEnumerator firingCooldown() {
        if (currentWeaponStats.fireDelay <= 0 || state.weaponCooling) {
            yield return null;
        }
        state.weaponCooling = true;

        // Fade the crosshair if we're not allowed to shoot.
        crossHair.color = new Color(1f, 1f, 1f, 0.3f);
        for (float i = 0; i < currentWeaponStats.fireDelay; i += Time.deltaTime) {
            crossHair.fillAmount = i/currentWeaponStats.fireDelay;
            yield return null;
        }

        state.weaponCooling = false;
        crossHair.fillAmount = 1;
        crossHair.color = new Color(1f, 1f, 1f, 0.8f);
        yield return null;
    }

    /// <summary>
    /// Updates weapon slot and enables weapon object
    /// </summary>
    public void LoadWeapon() {
        if ((guns == null || guns.Count-1 < slot) || ((weapon != null && weapon.activeSelf) && (guns[slot] == weapon || state.holsteringWeapon))) {
            return;
        }
        state.drawingWeapon = true;
        // Update weapon slot and enable weapon object
        // Shut up roslyn, Unity doesn't like conditional access here
        if (weapon != null) {
            weapon.SetActive(false);
        }

        // The weapon select functions already verify the new slot is good
        weapon = guns[slot];
        weapon.SetActive(true);
        currentWeaponStats = weapon.GetComponent<WeaponStats>();
    }

    private void playWeaponAnimation() {
        // If player is currently holstering or drawing a weapon, alter weapon position to animate the process.
        if (state.drawingWeapon) {
            if ((weapon.transform.parent.position - weapon.transform.position).magnitude > .1) {
                weapon.transform.position = (weapon.transform.position + weapon.transform.parent.position) / 2;
            }
            Vector3 rotationVector = transform.rotation.eulerAngles;
            rotationVector.x += currentWeaponStats.rotation;
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.parent.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
            // After animation has run set weapon changing to false
            if (animatedTime >= 0.9f) {
                animatedTime = 0f;
                state.drawingWeapon = false;
                crossHair.enabled = currentWeaponStats.showCrosshair;
            }
            animatedTime += Time.deltaTime;
        // ---------------------------------------------------------------------
        } else if (state.holsteringWeapon) {
            weapon.transform.position -= (transform.up * 5f) + transform.forward;
            Vector3 rotationVector = transform.rotation.eulerAngles;
            weapon.transform.rotation = Quaternion.Slerp(weapon.transform.parent.rotation, Quaternion.Euler(rotationVector), Time.fixedDeltaTime * 6);
            // After animation has run, get the new weapon
            if (animatedTime >= 0.8f) {
                animatedTime = 0f;
                state.holsteringWeapon = false;
                LoadWeapon();
            }
            animatedTime += Time.deltaTime;
        }
    }

    public void HolsterWeapon() {
        // If weapon is already selected, do nothing
        if (guns[slot] == weapon || state.drawingWeapon) {
            return;
        }
        state.holsteringWeapon = true;
        // Hide crossHair while weapon is changing
        crossHair.enabled = false;
    }
}
