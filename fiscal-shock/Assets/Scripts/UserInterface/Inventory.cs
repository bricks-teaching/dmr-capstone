using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace FiscalShock.GUI {
    public class Inventory : MonoBehaviour {
        public List<PlayerInventorySlot> inventorySlots;
        public AudioClip sellSound;
        private AudioSource audioS => GetComponent<AudioSource>();
        private PlayerShoot playerShoot;
        private ShopScript shopkeep;
        private int latestSlot = -1;

        public void FixedUpdate() {
            updateSelectedGrid();
        }

        private void updateSelectedGrid() {
            if (playerShoot != null && latestSlot != playerShoot.slot) {
                if (latestSlot >= 0) {
                    inventorySlots[latestSlot].vignette.color = Color.black;
                }
                latestSlot = playerShoot.slot;
                inventorySlots[latestSlot].vignette.color = Color.white;
            }
        }

        public void Start() {
            playerShoot = GameObject.FindGameObjectWithTag("Player")?.GetComponentInChildren<PlayerShoot>(true);
            updateWeaponSlots();
        }

        public void showWeaponInfo(int slot) {
            if (slot < playerShoot.guns.Count) {
                inventorySlots[slot].infoBlock.SetActive(true);
            }
        }

        public void hideWeaponInfo(int slot) {
            inventorySlots[slot].infoBlock.SetActive(false);
        }

        public void updateWeaponSlots() {
            if (playerShoot == null) {
                // try to find it again
                playerShoot = GameObject.FindGameObjectWithTag("Player")?.GetComponentInChildren<PlayerShoot>(true);
            }
            if (playerShoot.guns == null) {
                return;
            }
            // filled slots
            for (int i = 0; i < playerShoot.guns.Count; ++i) {
                updateWeaponSlot(i);
            }
            // empty slots
            for (int i = playerShoot.guns.Count; i < inventorySlots.Count; ++i) {
                PlayerInventorySlot p = inventorySlots[i];
                p.sprite.color = new Color(0, 0, 0, 0);
                p.infoBlock.SetActive(false);
            }
            updateSelectedGrid();
        }

        private void updateWeaponSlot(int slot) {
            PlayerInventorySlot p = inventorySlots[slot];
            WeaponStats w = playerShoot.guns[slot].GetComponentInChildren<WeaponStats>(true);

            p.sprite.sprite = w.image;
            p.sprite.color = new Color(1, 1, 1, 1);
            p.name.text = w.fullName;
            p.family.text = $"{w.actionToString()} {w.weaponFamily}";
            // Strength, jitter, delay
            p.statsLeft.text = $"Strength: {w.strength}\nDeviation: {w.deviation}\nDelay: {w.fireDelay}";
            // Projectile type, bullet cost, value (modify based on what the shopkeep will pay for it)
            p.statsRight.text = $"{w.projectileTypeToString()}\nBullet Cost: {w.bulletCost}\nValue: {w.buybackPrice}";
        }

        public void sellWeapon(int slot) {
            if (playerShoot == null) {
                // try to find it again
                playerShoot = GameObject.FindGameObjectWithTag("Player")?.GetComponentInChildren<PlayerShoot>(true);
            }
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Dungeon" && playerShoot.guns.Count > slot) {
                playerShoot.slot = slot;
                return;
            }
            if (playerShoot.guns.Count <= 1) {  // disallow selling last weapon
                return;
            }
            if (shopkeep == null) {
                GameObject shop = GameObject.Find("WeaponShop");
                if (shop == null) {
                    return;
                }
                shopkeep = shop.GetComponentInChildren<ShopScript>();
            }
            if (!shopkeep.playerIsInTriggerZone || playerShoot.guns.Count <= slot) {
                return;
            }
            GameObject gun = playerShoot.guns[slot];
            WeaponStats w = gun.GetComponentInChildren<WeaponStats>(true);

            playerShoot.guns.Remove(gun);
            StateManager.cashOnHand += w.buybackPrice;
            Debug.Log($"Sold {w.fullName} for {w.buybackPrice}");
            audioS.PlayOneShot(sellSound, Settings.volume);
            // Guns should be attached to a transform that positions them on the player
            Destroy(w.bulletPoolObject);
            Destroy(gun.transform.parent.gameObject);
            updateWeaponSlots();
        }

        public void addWeapon(GameObject selection) {
            if (playerShoot == null) {
                // try to find it again
                playerShoot = GameObject.FindGameObjectWithTag("Player")?.GetComponentInChildren<PlayerShoot>(true);
            }
            if (playerShoot.guns.Count > 9) {
                Debug.Log("You don't have enough arms and bags of holding for another weapon.");
                return;
            }
            // add the gun models!
            // when the camera is parented to the gun, the gun's transform is reset to be relative to the camera, so we need to fix it after the parenting
            Vector3 correctPosition = selection.transform.localPosition;
            Vector3 correctScale = selection.transform.localScale;
            Quaternion correctRotation = selection.transform.localRotation;
            selection.transform.parent = GameObject.FindGameObjectWithTag("MainCamera").transform;
            selection.transform.localPosition = correctPosition;
            selection.transform.localRotation= correctRotation;
            selection.transform.localScale = correctScale;
            // The gun model itself that needs to be added to the list is not the topmost object
            GameObject actualGun = selection.GetComponentInChildren<WeaponStats>(true).gameObject;
            playerShoot.guns.Add(actualGun);
            updateWeaponSlots();
        }
    }

    [System.Serializable]
    public class PlayerInventorySlot {
        public Image sprite;
        public TextMeshProUGUI name;
        public TextMeshProUGUI family;
        public TextMeshProUGUI statsLeft;
        public TextMeshProUGUI statsRight;
        public GameObject infoBlock;
        public Image vignette;
    }
}
