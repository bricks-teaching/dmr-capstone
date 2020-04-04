using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopScript : MonoBehaviour
{
    private AudioSource audioS;
    public AudioClip paymentSound;
    public AudioClip failureSound;
    public TextMeshProUGUI dialogText;
    public GameObject shopPanel;
    public Button hoseButton;
    public Button fishButton;
    public Button backButton;
    private bool playerIsInTriggerZone = false;

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            playerIsInTriggerZone = true;
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Player") {
            playerIsInTriggerZone = false;
        }
    }
    void Start() {
        audioS = GetComponent<AudioSource>();
        shopPanel.SetActive(false);
        Button btnOne = hoseButton.GetComponent<Button>();
        Button btnTwo = fishButton.GetComponent<Button>();
        Button btnThr = backButton.GetComponent<Button>();
		btnOne.onClick.AddListener(buyHose);
        btnTwo.onClick.AddListener(buyFish);
        btnThr.onClick.AddListener(BackClick);
    }
    // Update is called once per frame
    void Update()
    {
        if (playerIsInTriggerZone && Input.GetKeyDown(Settings.interactKey)) {
            Settings.forceUnlockCursorState();
            shopPanel.SetActive(true);
        }
    }

    public bool buyWeapon(int weapon, float cost){
        if(cost > PlayerFinance.cashOnHand){
            return false;
        }
        if(weapon == 0 && !StateManager.purchasedHose){
            StateManager.purchasedHose = true;
            PlayerFinance.cashOnHand -= cost;
        } else if(weapon == 1 && !StateManager.purchasedLauncher){
            StateManager.purchasedLauncher = true;
            PlayerFinance.cashOnHand -= cost;
        } else {
            return false;
        }
        return true;
    }

    void buyHose(){
        bool success = buyWeapon(0, 1000.0f);
        if(success){
            dialogText.text = "Alright, here ya go, try not to get yourself kilt. No really, I mean kilt, not killed, but don't do that either.";
            audioS.PlayOneShot(paymentSound, Settings.volume);
        } else if (StateManager.purchasedHose) {
            dialogText.text = "Do you really need two of those?";
            audioS.PlayOneShot(failureSound, Settings.volume);
        } else {
            dialogText.text = "You sure you have enough there, pal? I ain't running a charity here...";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }

    void buyFish(){
        bool success = buyWeapon(1, 1500.0f);
        if (success){
            dialogText.text = "Pretty weird that the only way to make money around here is scrapping robots, innit?";
            audioS.PlayOneShot(paymentSound, Settings.volume);
        } else if (StateManager.purchasedLauncher) {
            dialogText.text = "Sorry, man, out of stock. Good ol' Mr. Popper came by earlier.";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        } else {
            dialogText.text = "I think you are a bit short today. Go scrap some bots and come back.";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }
    public void BackClick()
    {
        dialogText.text = "What are ya buyin'?";
        shopPanel.SetActive(false);
        Settings.forceLockCursorState();
    }
}
