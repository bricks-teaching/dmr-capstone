using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FiscalShock.Procedural;
using FiscalShock.GUI;

/// <summary>
/// This is the script that runs the shop. Weapons are generated and 
/// the shop sells these to the player.
/// </summary>
public class ShopScript : MonoBehaviour {
    public GameObject tutorial;
    public GameObject debugMenu;
    private AudioSource audioS;
    public AudioClip paymentSound;
    public AudioClip failureSound;
    public TextMeshProUGUI dialogText;
    public GameObject shopPanel;
    private WeaponGenerator genny;
    public List<ShopInventorySlot> inventorySlots;
    public bool playerIsInTriggerZone { get; private set; } = false;
    private List<GameObject> inventory = new List<GameObject>();
    private PlayerShoot playerShoot;
    private Inventory playerInventory;
    private readonly int MAX_GUNS = 9;
    /// <summary>
    /// Dialog options for the shopkeeper, selected at random.
    /// </summary>
    private readonly string[] purchaseDialogs = {
        "Alright, here ya go, try not to get yourself kilt. No, really, I mean kilt, not killed, but don't do that either.",
        "Pretty weird that the only way to make money around here is scrapping robots, innit?"
    };
    private readonly string[] brokeDialogs = {
        "You sure you have enough there, pal? I ain't running a charity here...",
        "I think you are a bit short today. Go scrap some bots and come back."
    };
    private readonly string[] fullInventoryDialogs = {
        "Greedy, aren'cha? Why don't you lend me some of your stock?",
        "I don't think you could fit another in your extra-dimensional satchel.",
        "I'll pay more than anybody for your leftovers. 10% is still more than 0%!"
    };

    /// <summary>
    /// Selects a dialog string from the dialog sets up above.
    /// </summary>
    private string getRandomDialog(string[] dialogs) {
        int val = Random.Range(0, dialogs.Length);
        return dialogs[val];
    }

    /// <summary>
    /// Detects the player and shows the tutorial if not already seen.
    /// </summary>
    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            playerIsInTriggerZone = true;
            if (!Settings.values.sawShopTutorial) {
                Time.timeScale = 0;
                Settings.forceUnlockCursorState();
                tutorial.SetActive(true);
                Settings.values.sawShopTutorial = true;
            }
        }
    }

    /// <summary>
    /// Dismisses the tutorial and allows normal gameplay to continue.
    /// </summary>
    public void dismissTutorial() {
        tutorial.SetActive(false);
        Time.timeScale = 1;
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Player") {
            playerIsInTriggerZone = false;
        }
    }

    /// <summary>
    /// Sets up the players inventory and shotting script for later use.
    /// Generates weapons for the shops inventory whenever the player enters the hub.
    /// </summary>
    void Start() {
        audioS = GetComponent<AudioSource>();
        shopPanel.SetActive(false);
        playerShoot = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerShoot>();
        playerInventory = GameObject.FindGameObjectWithTag("Player Inventory").GetComponentInChildren<Inventory>();
        genny = GetComponent<WeaponGenerator>();
        do {
            generateNewWeapons();
        } while (!affordableWeaponsDayZero());
    }

    /// <summary>
    /// With random generation, the player could, in rare cases, be unable to
    /// purchase any weapons on day 0 (due to loan caps). Make sure there's at
    /// least one weapon that's a reasonable price on day 0.
    /// </summary>
    private bool affordableWeaponsDayZero() {
        if (StateManager.sawEntryTutorial) {
            return true;
        }
        const float maxAllowedValue = 1333f;
        bool acceptablePrices = false;
        foreach (GameObject weapon in inventory) {
            WeaponStats stats = weapon.GetComponentInChildren<WeaponStats>(true);
            if (stats.price <= maxAllowedValue) {
                acceptablePrices = true;
                break;
            }
        }
        return acceptablePrices;
    }

    /// <summary>
    /// Generates new weapons in the shop. Every slot in the inventory gets
    /// a new weapon and all buttons and markers are set to defaults.
    /// </summary>
    public void generateNewWeapons() {
        Debug.Log($"Populating {inventorySlots.Count} shop slots");
        inventory.Clear();
        foreach (ShopInventorySlot slot in inventorySlots) {
            GameObject ran = genny.generateRandomWeapon();
            WeaponStats stats = ran.GetComponentInChildren<WeaponStats>(true);
            inventory.Add(ran);

            slot.buttonObject.GetComponent<Button>().image.sprite = stats.image;

            slot.purchased = false;
            slot.soldout.SetActive(false);
            slot.buttonObject.GetComponent<Button>().interactable = true;
            slot.name.text = stats.fullName;
            slot.pricetag.text = stats.price.ToString("F2");
            slot.family.text = $"{stats.actionToString()} {stats.weaponFamily}";
            slot.stats.text = $"Projectile: {stats.projectileTypeToString()}\nStrength: {stats.strength}\nDeviation: {stats.deviation}\nDelay: {stats.fireDelay}\nPrice per Shot: {stats.bulletCost}";
        }
    }

    public void hoverInventory(int slot) {
        if (!inventorySlots[slot].purchased) {
            inventorySlots[slot].infoBlock.SetActive(true);
        }
    }

    public void blurInventory(int slot) {
        inventorySlots[slot].infoBlock.SetActive(false);
    }


    /// <summary>
    /// Determines if the player is in range of the shop and pressing the interact key.
    /// If so, it activates the menu and unlocks the cursor.
    /// </summary>
    void Update() {
        if (playerIsInTriggerZone) {
            if (Input.GetKeyDown(Settings.interactKey) && !tutorial.activeSelf) {
                Time.timeScale = 0;
                Settings.forceUnlockCursorState();
                shopPanel.SetActive(true);
                StateManager.pauseAvailable = false;
            }
            if (Input.GetKeyDown(Settings.pauseKey)) {
                BackClick();
            }
            if (Input.GetKeyDown("f5")) {
                debugMenu.SetActive(!debugMenu.activeSelf);
            }
        }
    }

    /// <summary>
    /// Attempts to purchase a weapon and displays dialog based on the result
    /// of the attempt.
    /// </summary>
    /// <param name="slot"></param>
    public void purchaseWeapon(int slot) {
        ShopInventorySlot shopSlot = inventorySlots[slot];
        GameObject selection = inventory[slot];
        WeaponStats stats = selection.GetComponentInChildren<WeaponStats>(true);

        if (stats.price > StateManager.cashOnHand) {
            audioS.PlayOneShot(failureSound, Settings.volume);
            dialogText.text = getRandomDialog(brokeDialogs);
            return;
        }

        if (playerShoot.guns.Count >= MAX_GUNS) {
            audioS.PlayOneShot(failureSound, Settings.volume);
            dialogText.text = getRandomDialog(fullInventoryDialogs);
            return;
        }

        // can purchase
        playerInventory.addWeapon(selection);
        shopSlot.purchased = true;

        // only remove cash if everything above that succeeded and the player received the gun
        StateManager.cashOnHand -= stats.price;
        shopSlot.buttonObject.GetComponent<Button>().interactable = false;
        shopSlot.infoBlock.SetActive(false);
        shopSlot.soldout.SetActive(true);
        shopSlot.pricetag.text = "-";
        audioS.PlayOneShot(paymentSound, Settings.volume);
        dialogText.text = getRandomDialog(purchaseDialogs);
    }

    /// <summary>
    /// On click this method activates, granting the user control of
    /// the mouse and returning them to the hub.
    /// </summary>
    public void BackClick()
    {
        dialogText.text = "What are ya buyin'?";
        shopPanel.SetActive(false);
        tutorial.SetActive(false);
        StateManager.pauseAvailable = true;
        Settings.forceLockCursorState();
        Time.timeScale = 1;
    }

    // Debug stuff!!!
    public void setStateTimesEntered(TMP_InputField inp) {
        int val = int.Parse(inp.text);
        StateManager.timesEntered = val;
        Debug.Log($"StateManager.timesEntered: {StateManager.timesEntered}");
    }

    public void setStateTotalFloors(TMP_InputField inp) {
        int val = int.Parse(inp.text);
        StateManager.totalFloorsVisited = val;
        Debug.Log($"StateManager.timesEntered: {StateManager.totalFloorsVisited}");
    }
}

/// <summary>
/// Data for each shop inventory slot. Shows some basic text and data
/// about the weapon in question
/// </summary>
[System.Serializable]
public class ShopInventorySlot {
    public GameObject buttonObject;
    public TextMeshProUGUI pricetag;
    public TextMeshProUGUI name;
    public TextMeshProUGUI family;
    public TextMeshProUGUI stats;
    public GameObject infoBlock;
    public GameObject soldout;
    public bool purchased;
}
