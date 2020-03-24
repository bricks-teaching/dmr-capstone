using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script holds information about a weapon
public class WeaponStats : MonoBehaviour
{
    public int strength = 10;
    public int bulletCost = 1;
    public float accuracy = 6;
    public float rotation = 0;
    public bool continuous = false;
    public bool missile = false;
    public GameObject bulletPrefab;
    public bool usingPool { get; private set; }
    public bool showCrosshair = true;

    public Queue<GameObject> bulletPool = new Queue<GameObject>();

    void Start() {
        BulletBehavior bb = bulletPrefab.GetComponent<BulletBehavior>();
        if (bb.poolProjectiles) {
            for (int i = 0; i < bb.poolSize; ++i) {
                GameObject boolet = Instantiate(bulletPrefab, bulletPrefab.transform.position, bulletPrefab.transform.rotation);
                boolet.transform.parent = transform;
                boolet.SetActive(false);
                bulletPool.Enqueue(boolet);
            }
            usingPool = true;
        }
    }
}
