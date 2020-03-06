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
    public bool missle = false;
    public GameObject bulletPrefab;
}
