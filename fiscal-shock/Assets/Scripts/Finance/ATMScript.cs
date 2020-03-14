using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
//using System;

public class ATMScript : MonoBehaviour {
    public AudioClip paymentSound;
    public AudioClip failureSound;
    public static bool bankDue = true;
    private bool playerIsInTriggerZone = false;
    private TextMeshProUGUI signText;
    private string defaultSignText;
    private AudioSource audio;

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            playerIsInTriggerZone = true;
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Player") {
            playerIsInTriggerZone = false;
            signText.text = defaultSignText;
        }
    }

    void Start() {
        signText = GetComponentInChildren<TextMeshProUGUI>();
        defaultSignText = signText.text.Replace("INTERACTKEY", Settings.interactKey.ToUpper());
        signText.text = defaultSignText;
        audio = GetComponent<AudioSource>();
    }

    void Update() {
        if (playerIsInTriggerZone && Input.GetKeyDown(Settings.interactKey)) {
            bool paymentSuccessful = payDebt(100);
            if (paymentSuccessful) {
                signText.text = "";
                audio.PlayOneShot(paymentSound, Settings.volume);
                Debug.Log("Paid $100");
            } else {
                signText.text = "Please tender payments using cash, not respects.";
                audio.PlayOneShot(failureSound, Settings.volume * 2.5f);
                Debug.Log("Not enough cash to pay denbts");
            }
        }
    }

    public bool addDebt(float amount) {
        if (PlayerFinance.bankThreatLevel < 3 && PlayerFinance.bankMaxLoan > (PlayerFinance.debtBank + amount)) {
            // bank threat is below 3 and is below max total debt
            PlayerFinance.debtBank += amount;
            PlayerFinance.cashOnHand += amount;
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(float amount) {
        if (PlayerFinance.cashOnHand < amount) { // amount is more than money on hand
            //display a message stating error
            return false;
        } else if (PlayerFinance.debtBank < amount) { // amount is more than the debt
            PlayerFinance.debtBank = 0.0f; // reduce debt to 0 and money on hand by the debt's value
            PlayerFinance.cashOnHand -= PlayerFinance.debtBank;
            bankDue = false;
            temporaryWinGame();
            return true;
        } else { // none of the above
            // reduce debt and money by amount
            PlayerFinance.debtBank -= amount;
            PlayerFinance.cashOnHand -= amount;
            bankDue = false;
            return true;
        }
    }

    public void temporaryWinGame() {
        SceneManager.LoadScene("WinGame");
    }
}
