﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ATMScript : MonoBehaviour {
    public static bool bankDue = true;
    private bool playerIsInTriggerZone = false;
    private TextMeshProUGUI signText;
    private string defaultSignText;

    void OnTriggerEnter() {
        Debug.Log($"{gameObject.name}: Triggered");
        playerIsInTriggerZone = true;
    }

    void OnTriggerExit() {
        Debug.Log($"{gameObject.name}: Left ATM");
        playerIsInTriggerZone = false;
        signText.text = defaultSignText;
    }

    void Start() {
        signText = GetComponentInChildren<TextMeshProUGUI>();
        defaultSignText = signText.text;
    }

    void FixedUpdate() {
        if (playerIsInTriggerZone && Input.GetKeyDown("f")) {
            bool paymentSuccessful = payDebt(100);
            if (paymentSuccessful) {
                signText.text = "$$$";
                Debug.Log("Paid $100");
            } else {
                signText.text = "Please tender payments using cash, not wishes and dreams.";
                Debug.Log("Son u broke");
            }
        }
    }

    public bool addDebt(float amount) {
        if (PlayerFinance.bankThreatLevel < 3 && PlayerFinance.bankMaxLoan > (PlayerFinance.debtBank + amount)){
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
