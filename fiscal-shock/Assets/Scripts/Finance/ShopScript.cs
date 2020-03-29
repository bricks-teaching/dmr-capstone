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
    private TextMeshProUGUI dialogText;
    public GameObject shopPanel;
    public Button hoseButton;
    public Button fishButton;
    private bool playerIsInTriggerZone = false;
    private bool hoseBought = false;
    private bool launcherBought = false;

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
    void Start(){
        audioS = GetComponent<AudioSource>();
        shopPanel.SetActive(false);
        Button btnOne = hoseButton.GetComponent<Button>();
        Button btnTwo = fishButton.GetComponent<Button>();
		btnOne.onClick.AddListener(buyHose);
        btnTwo.onClick.AddListener(buyFish);
    }
    // Update is called once per frame
    void Update()
    {
        if (playerIsInTriggerZone && Input.GetKeyDown(Settings.interactKey)) {
            Settings.mutexUnlockCursorState(this);
            shopPanel.SetActive(true);
        }
    }

    public bool buyWeapon(int weapon, float cost){
        if(cost > PlayerFinance.cashOnHand){
            return false;
        }
        if(weapon == 0 && !hoseBought){
            PlayerShoot.slotZero = true;
            hoseBought = true;
            PlayerFinance.cashOnHand -= cost;
        } else if(weapon == 1 && !launcherBought){
            PlayerShoot.slotOne = true;
            launcherBought = true;
            PlayerFinance.cashOnHand -= cost;
        } else {
            return false;
        }
        return true;
    }

    void buyHose(){
        bool success = buyWeapon(0, 1000.0f);
        if(success){
            dialogText.text = "";
            audioS.PlayOneShot(paymentSound, Settings.volume);
        } else {
            dialogText.text = "";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }

    void buyFish(){
        bool success = buyWeapon(1, 1500.0f);
        if(success){
            dialogText.text = "";
            audioS.PlayOneShot(paymentSound, Settings.volume);
        } else {
            dialogText.text = "";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }
    public void BackClick()
    {
        shopPanel.SetActive(false);
    }
}
