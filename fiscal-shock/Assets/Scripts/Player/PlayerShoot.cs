using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    private AudioSource fireSound;
    public AudioClip fireSoundClip;
    public GameObject weapon;
    public GameObject controller;
    public Text pocketChange;
    public float volume = 1f;
    public bool weaponChanging = true;
    // Start is called before the first frame update
    void Start()
    {
        fireSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && !weaponChanging && Time.timeScale > 0)
        {
            WeaponStats weaponScript = weapon.GetComponent(typeof(WeaponStats)) as WeaponStats;
            if(weaponScript.ammo > 0)
            {
                fireBullet(10 - weaponScript.accuracy, weaponScript.strength);
                WeaponDemo mainScript = controller.GetComponent(typeof(WeaponDemo)) as WeaponDemo;
                mainScript.changeMoney(-weaponScript.bulletCost);
                Debug.Log("Cost: $" + weaponScript.bulletCost + " You have " + weaponScript.ammo + " bullets remaining");
            }
        }
    }

    private void fireBullet(float accuracy, int damage)
    {
        fireSound.PlayOneShot(fireSoundClip, volume);
        GameObject bullet = Instantiate(bulletPrefab, gameObject.transform.position + (gameObject.transform.forward * 50), gameObject.transform.rotation) as GameObject;
        BulletBehavior bulletScript = bullet.GetComponent(typeof(BulletBehavior)) as BulletBehavior;
        bulletScript.damage = damage;
        Vector3 rotationVector = bullet.transform.rotation.eulerAngles;
        rotationVector.x += ((Random.value * 2) - 1) * accuracy;
        rotationVector.y += ((Random.value * 2) - 1) * accuracy;
        rotationVector.z += ((Random.value * 2) - 1) * accuracy;
        bullet.transform.rotation = Quaternion.Euler(rotationVector);
    }
}
