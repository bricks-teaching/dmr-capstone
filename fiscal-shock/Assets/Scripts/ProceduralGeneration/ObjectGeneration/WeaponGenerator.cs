using UnityEngine;
using System.Collections.Generic;

namespace FiscalShock.Procedural {
    public class WeaponGenerator : MonoBehaviour {
        public List<RandomWeapon> randomWeapons;

        public GameObject generateRandomWeapon() {
            // select a base
            int randi;
            float die;

            do {
                randi = Random.Range(0, randomWeapons.Count);
                die = Random.Range(0f, 1f);
            } while (die < randomWeapons[randi].rarity);

            reroll(ref randi, ref die, randomWeapons);

            // instantiate the selected prefab
            RandomWeapon choice = randomWeapons[randi];
            GameObject wapon = Instantiate(choice.prefab);
            // the child is probably disabled, so pass true to search inactive
            WeaponStats stats = wapon.GetComponentInChildren<WeaponStats>(true);

            // randomize stuff
            // pick projectile configuration
            reroll(ref randi, ref die, choice.projectileConfigurations);
            ProjectileCombination combo = choice.projectileConfigurations[randi];
            // update weaponstats
            stats.bulletPrefab = combo.bulletPrefab;
            stats.action = combo.firearmAction;
            stats.projectileType = combo.projectileType;
            stats.showCrosshair = combo.showCrosshair;
            float valueModifier = 1;

            // randomize stats
            // --- Strength ---
            float strMod = getStrengthModifier();
            stats.strength = randomizeAndClamp(combo.strengthRange, strMod);
            float modifiedStrDev = strMod * combo.strengthRange.w * 0.5f;
            float diff = stats.strength - (combo.strengthRange.z * strMod);
            if (diff > modifiedStrDev) {  // unusually strong
                stats.weaponName = getRandomString(choice.strongNames);
            } else if (diff < -modifiedStrDev) {  // unusually weak
                stats.weaponName = getRandomString(choice.weakNames);
            } else {
                stats.weaponName = getRandomString(choice.boringNames);
            }
            valueModifier *= stats.strength/combo.strengthRange.z;

            // --- Accuracy ---
            stats.deviation = randomizeAndClamp(combo.accuracyRange);
            diff = stats.deviation - combo.accuracyRange.z;
            if (diff > combo.accuracyRange.w) {  // unusually high jitter
                stats.suffix = getRandomString(choice.inaccurateSuffixes);
            } else if (diff < -combo.accuracyRange.w) {  // unusually low jitter
                stats.suffix = getRandomString(choice.accurateSuffixes);
            }
            // Deviation should affect the price less.
            valueModifier *= Mathf.Clamp(combo.accuracyRange.z/((stats.deviation + 1) * 5f), 0.1f, Mathf.Log(Mathf.Pow(StateManager.timesEntered, 2) + 20));

            // --- Speed ---
            stats.fireDelay = randomizeAndClamp(combo.fireDelayRange);
            diff = stats.fireDelay - combo.fireDelayRange.z;
            if (diff > combo.fireDelayRange.w) {  // unusually high speed
                stats.prefix = getRandomString(choice.highFireRatePrefixes);
            } else if (diff < -combo.accuracyRange.w) {  // unusually low speed
                stats.prefix = getRandomString(choice.lowFireRatePrefixes);
            }
            if (stats.fireDelay >= 0.1f) {  // clear out looping clip
                stats.gunAudioSource.clip = null;
            }
            valueModifier *= Mathf.Clamp(combo.fireDelayRange.z/(stats.fireDelay + 0.05f), 0.1f, Mathf.Log(Mathf.Pow(StateManager.timesEntered, 2) + 20));

            stats.bulletCost = getBulletCost(stats.strength);
            // pricing:
            // auto - increase
            // homing - increase
            // so think of semiauto bullet as baseline
            if (stats.action == FirearmAction.Automatic) {
                valueModifier *= 2f;
            }
            if (stats.projectileType == ProjectileType.HomingMissile) {
                valueModifier *= 1.5f;
            }

            wapon.name = stats.fullName;

            valueModifier *= (float)System.Math.Log(StateManager.timesEntered+2)
            ;
            float originalPrice = stats.price;
            stats.price *= (float)System.Math.Round(valueModifier, 2);
            stats.price += originalPrice;
            Debug.Log($"{stats}\nGenerated with value mod of {valueModifier}");

            return wapon;
        }

        /// <summary>
        /// Get a random string out of the list
        /// </summary>
        /// <param name="strung"></param>
        /// <returns></returns>
        private string getRandomString(List<string> strung) {
            return strung[Random.Range(0, strung.Count)];
        }

        /// <summary>
        /// Get a random index for a base weapon type.
        /// Mutates input
        /// </summary>
        /// <param name="index"></param>
        /// <param name="diceRoll"></param>
        /// <param name="stuff"></param>
        private void reroll(ref int index, ref float diceRoll, List<RandomWeapon> stuff) {
            do {
                index = Random.Range(0, stuff.Count);
                diceRoll = Random.Range(0f, 1f);
            } while (diceRoll < stuff[index].rarity);
        }

        /// <summary>
        /// Get a random index for projectile combination.
        /// Mutates input
        /// </summary>
        /// <param name="index"></param>
        /// <param name="diceRoll"></param>
        /// <param name="stuff"></param>
        private void reroll(ref int index, ref float diceRoll, List<ProjectileCombination> stuff) {
            do {
                index = Random.Range(0, stuff.Count);
                diceRoll = Random.Range(0f, 1f);
            } while (diceRoll < stuff[index].rarity);
        }

        /// <summary>
        /// Generate a random value from the Gaussian distribution.
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        private float randomizeAndClamp(Vector4 parms) {
            return (float)System.Math.Round(Mathf.Clamp(Gaussian.next(parms.z, parms.w), parms.x, parms.y), 2);
        }

        /// <summary>
        /// Generate a random value from the Gaussian distribution after
        /// modifying the parameters.
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        private float randomizeAndClamp(Vector4 parms, float modifier) {
            return (float)System.Math.Round(Mathf.Clamp(Gaussian.next(parms.z * modifier, (parms.w * modifier) * 0.5f), parms.x * modifier, parms.y * modifier), 2);
        }

        /// <summary>
        /// Specialized formula to get a curve that lets us hit 1 at x = 0, ~2
        /// at x = 5, ~3 at x = 15, ~4 at x = 30, ~5 at x = 65.
        /// Found by using a curve fitting tool and adjusting values.
        /// https://mycurvefit.com/
        /// <para>Formula: 7 + (-6/(1 + (x/32)^0.9))</para>
        /// </summary>
        /// <returns></returns>
        private float getStrengthModifier() {
            float x = StateManager.totalFloorsVisited + 1;
            return 7 + (-6/(1 + Mathf.Pow(x/32, 0.9f)));
        }

        /// <summary>
        /// Specialized formula to calculate bullet based on damage.
        /// ~1 when x = 2, ~6 when x = 10, ~22 when x = 50, ~36 when x = 100.
        /// </summary>
        /// <param name="strength"></param>
        /// <returns></returns>
        private float getBulletCost(float strength) {
            return (float)System.Math.Round(1375 - (1374/(0.998f + Mathf.Pow(strength/16000, 0.7f))), 2);
        }
    }
}

[System.Serializable]
public class ProjectileCombination {
    public GameObject bulletPrefab;
    public FirearmAction firearmAction;
    public ProjectileType projectileType;
    public bool showCrosshair;

    [Tooltip("Rarity of this configuration.")]
    [Range(0, 1)] public float rarity;

    [Tooltip("Statistical distribution of data. Stats are selected from a Gaussian (normal) distribution. Most values will be near the mean.")]
    [Header("Min (X), Max (Y), Mean (Z), StdDev (W)")]
    public Vector4 strengthRange;
    public Vector4 accuracyRange;
    public Vector4 fireDelayRange;
}

[System.Serializable]
public class RandomWeapon {
    [Header("Main Settings")]
    [Tooltip("Base prefab. Should contain everything from the weapon position transform down and have a WeaponStats attached to the gun.")]
    public GameObject prefab;

    [Tooltip("How rare this base weapon type is.")]
    [Range(0, 1)] public float rarity;

    [Tooltip("Valid mechanic combinations.")]
    public List<ProjectileCombination> projectileConfigurations;

    [Header("Flavor")]
    public string weaponFamily;

    [Tooltip("Names allowed for weapons that don't fall outside normal values.")]
    public List<string> boringNames;

    [Tooltip("Names allowed for weapons with low strength.")]
    public List<string> weakNames;

    [Tooltip("Names allowed for weapons with high strength.")]
    public List<string> strongNames;

    [Tooltip("Suffixes allowed for weapons with poor accuracy.")]
    public List<string> inaccurateSuffixes;

    [Tooltip("Suffixes allowed for weapons with high accuracy.")]
    public List<string> accurateSuffixes;

    [Tooltip("Suffixes allowed for weapons with low fire rates.")]
    public List<string> lowFireRatePrefixes;

    [Tooltip("Suffixes allowed for weapons with high fire rates.")]
    public List<string> highFireRatePrefixes;
}
