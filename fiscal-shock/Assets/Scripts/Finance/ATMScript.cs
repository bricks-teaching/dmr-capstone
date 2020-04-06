using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

public class ATMScript : MonoBehaviour {
    public AudioClip paymentSound;
    public AudioClip failureSound;
    private bool playerIsInTriggerZone = false;
    private int loanCount => StateManager.loanList.Count(l => l.source != LoanType.Payday);
    private AudioSource audioS;
    public TextMeshProUGUI dialogText, rateText;
    public GameObject bankPanel;
    public TMP_InputField paymentId, paymentAmount;
    public List<LoanEntry> loanEntries;
    public static float bankMaxLoan { get; set; } = 10000.0f;
    public static float bankInterestRate { get; set; } = 0.035f;
    public static int bankThreatLevel { get; set; } = 0;
    public static float bankTotal => StateManager.loanList.Where(l => l.source != LoanType.Payday).Sum(l => l.total);
    public static float securedAmount { get; set; } = 1.15f;
    public static float rateReducer { get; set; } = 0.85f;
    private static List<Loan> bankLoans => StateManager.loanList.Where(l => l.source != LoanType.Payday).ToList();

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
        bankPanel.SetActive(false);
        if (!StateManager.sawTutorial) {  // implies this is the first visit to town
            addDebt(2000.0f, LoanType.Unsecured);
            StateManager.cashOnHand = 900f;
        }
        updateFields();
        rateText.text = $"Today's rates: {bankInterestRate * 100}%";
    }

    void Update() {
        if (playerIsInTriggerZone) {
            if (Input.GetKeyDown(Settings.interactKey)) {
                Settings.forceUnlockCursorState();
                updateFields();
                bankPanel.SetActive(true);
                StateManager.pauseAvailable = false;
            }
            if (Input.GetKeyDown(Settings.pauseKey)) {
                BackClick();
            }
        }
    }

    public bool addDebt(float amount, LoanType loanType) {
        if (amount < 0.0f) {
            return false;
        }
        if (bankThreatLevel < 3 && bankMaxLoan > (bankTotal + amount) && loanCount < 3) {
            // bank threat is below 3 and is below max total debt
            Loan newLoan = null;
            switch (loanType) {
                case LoanType.Unsecured:
                    newLoan = new Loan(StateManager.nextID, amount, bankInterestRate, loanType);
                    break;
                case LoanType.Secured:
                    newLoan = new Loan(StateManager.nextID, amount * securedAmount, bankInterestRate * rateReducer, loanType);
                    break;
                default:
                    return false; //this shouldnt activate, false return is a failsafe measure
            }
            StateManager.loanList.AddLast(newLoan);
            StateManager.cashOnHand += amount;
            StateManager.nextID++;
            updateFields();
            return true;
        }
        else {
            return false;
        }
    }

    public bool payDebt(float amount, int loanNum) {
        Loan selectedLoan = bankLoans.First(l => l.ID == loanNum);
        if (amount < 0.0f || selectedLoan.Equals(null)) {
            return false;
        }
        if (StateManager.cashOnHand < amount) { // amount is more than money on hand
            //display a message stating error
            return false;
        }
        else if (selectedLoan.total <= amount) { // amount is more than the debt
            StateManager.cashOnHand -= selectedLoan.total;
            StateManager.loanList.Remove(selectedLoan);
            updateFields();
            return true;
        }
        else { // none of the above
            // reduce debt and money by amount
            selectedLoan.total -= amount;
            StateManager.cashOnHand -= amount;
            updateFields();
            return true;
        }
    }

    public static void bankUnpaid() {
        bool paid = true;
        foreach (Loan item in bankLoans) {
            if (!item.paid) {
                bankThreatLevel++;
                StateManager.paymentStreak = 0;
                paid = false;
            }
        }
        if (paid) {
            StateManager.paymentStreak++;
            bankThreatLevel--;
        }
    }

    public static void bankInterest() {
        foreach (Loan item in bankLoans) {
            item.paid = false;
            item.total += item.rate * item.total;
        }
    }

    public void temporaryWinGame() {
        SceneManager.LoadScene("WinGame");
    }

    public void payLoan() {
        try {
            float amount = float.Parse(paymentAmount.text, CultureInfo.InvariantCulture.NumberFormat);
            int ID = int.Parse(paymentId.text);
            if (payDebt(amount, ID)) {
                dialogText.text = "Thank you for your payment!";
                audioS.PlayOneShot(paymentSound, Settings.volume);
            }
            else {
                dialogText.text = "You don't have the money on you.";
                audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning($"{e.Message}");
            dialogText.text = "I'm afraid I don't understand what you're trying to do.";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }

    public void addLoan(TMP_InputField textField) {
        try {
            float amount = float.Parse(textField.text, CultureInfo.InvariantCulture.NumberFormat);
            if (addDebt(amount, LoanType.Unsecured)) {
                dialogText.text = "All set!";
                audioS.PlayOneShot(paymentSound, Settings.volume);
            }
            else {
                dialogText.text = "Hmm... I would suggest paying off previous debts first.";
                audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning($"{e.Message}");
            dialogText.text = "Perhaps you meant to give me a number?";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }
    public void addSecLoan(TMP_InputField textField) {
        try {
            float amount = float.Parse(textField.text, CultureInfo.InvariantCulture.NumberFormat);
            if (addDebt(amount, LoanType.Secured)) {
                dialogText.text = "YOUR SOUL IS MINE! Erm, I mean... All Set!";
                audioS.PlayOneShot(paymentSound, Settings.volume);
            }
            else {
                dialogText.text = "Nope, declined.";
                audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning($"{e.Message}");
            dialogText.text = "I apologize for the confusion. I can only accept amounts in base ten.";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }

    void updateFields() {
        for (int i = 0; i < loanEntries.Count; ++i) {
            if (i >= bankLoans.Count) {
                loanEntries[i].id.text = "";
                loanEntries[i].amount.text = "";
                loanEntries[i].type.text = "";
            }
            else {
                loanEntries[i].id.text = bankLoans[i].ID.ToString();
                loanEntries[i].amount.text = bankLoans[i].total.ToString();
                loanEntries[i].type.text = $"{(bankLoans[i].source == LoanType.Secured ? "Secured" : "Unsecured")}";
            }
        }
    }

    public void BackClick() {
        dialogText.text = "How may I help you?";
        bankPanel.SetActive(false);
        Settings.forceLockCursorState();
        StateManager.pauseAvailable = true;
    }
}

[System.Serializable]
public class LoanEntry {
    public TextMeshProUGUI id, type, amount;
}
