using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script holds information about a weapon
public class WeaponStats : MonoBehaviour
{
    public int strength = 10;
    public int bulletCost = 1;
    public float accuracy = 6;
    public float fireRate = 0;  // 0 = no delay
    public float rotation = 0;
    public FirearmAction action;
    public ProjectileType projectileType;  // Not part of bullet behavior right now, since the logic is in the shoot scripts
    public GameObject bulletPrefab;
    public bool showCrosshair = true;
    public Transform projectileSpawnPoint;

    public Queue<GameObject> bulletPool = new Queue<GameObject>();

    void Start() {
        BulletBehavior bb = bulletPrefab.GetComponent<BulletBehavior>();
        if (bb.poolSize < 1) {
            Debug.LogError($"Bullet prefab must have a pool size for player-fired bullets!");
        }
        GameObject pool = new GameObject();
        pool.name = $"{gameObject.name}'s Projectile Pool";
        for (int i = 0; i < bb.poolSize; ++i) {
            GameObject boolet = Instantiate(bulletPrefab, bulletPrefab.transform.position, bulletPrefab.transform.rotation);
            boolet.transform.parent = pool.transform;
            boolet.SetActive(false);
            bulletPool.Enqueue(boolet);
        }
        DontDestroyOnLoad(pool);
    }
}

public enum FirearmAction {
    SingleShot,
    Automatic,
    Semiautomatic
}

public enum ProjectileType {
    Bullet,
    HomingMissile
}
