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
    private AudioSource audioS;
    public TextMeshProUGUI dialogText, rateText;
    public GameObject bankPanel;
    public TMP_InputField paymentId, paymentAmount;
    public List<LoanEntry> loanEntries;
    public static List<Loan> bankLoans => StateManager.loanList.Where(l => l.source != LoanType.Payday).ToList();
    public int loanCount => bankLoans.Count;
    public static float bankTotal => bankLoans.Sum(l => l.total);
    /// <summary>
    /// Bank will loan more based on how many times you've been in dungeon.
    /// Quick fix; can be abused if player pays off loans then just repeatedly
    /// re-enters and the max loan keeps going up. Checks and offsets are
    /// because ln(n < 3) < 1
    /// </summary>
    /// <returns></returns>
    public static float bankMaxLoan => 2600.0f * (StateManager.timesEntered > 0? Mathf.Log(StateManager.timesEntered + 2) : 1);
    public static float bankInterestRate { get; set; } = 0.035f;
    public static int bankThreatLevel { get; set; } = 0;
    public static float securedAmount { get; set; } = 1.15f;
    public static float rateReducer { get; set; } = 0.75f;

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
        if (!StateManager.sawEntryTutorial) {  // implies this is the first visit to town
            addDebt(2000.0f, LoanType.Unsecured);
            StateManager.cashOnHand = 900f;
        }
        updateFields();
        rateText.text = $"Our flexible loan options are always backed by the UDIC.\nUnsecured: {bankInterestRate * 100}%\nSecured: {bankInterestRate * rateReducer * 100}% + {(securedAmount-1)*100}% of amount";
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
        if (bankThreatLevel < 3 && bankMaxLoan >= (bankTotal + amount) && loanCount < 3) {
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
            StateManager.cashOnHand += selectedLoan.collateral;  // get back extra amount paid on secured loans
            StateManager.loanList.Remove(selectedLoan);
            checkWin();
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

    public void checkWin() {
        if (StateManager.loanList.Count == 0) {
            StateManager.playerWon = true;
            GameObject.FindGameObjectWithTag("Loading Screen").GetComponent<LoadingScreen>().startLoadingScreen("WinGame");
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

    public void payLoan() {
        try {
            float amount = float.Parse(paymentAmount.text, CultureInfo.InvariantCulture.NumberFormat);
            int ID = int.Parse(paymentId.text);
            if (payDebt(amount, ID)) {
                dialogText.text = "Thank you for your payment!";
                audioS.PlayOneShot(paymentSound, Settings.volume);
                paymentAmount.text = "";  // clear text field
                paymentId.text = "";
            }
            else {
                dialogText.text = "You don't have the money on you.";
                audioS.PlayOneShot(failureSound, Settings.volume);
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning($"{e.Message}");
            dialogText.text = "I'm afraid I don't understand what you're trying to do.";
            audioS.PlayOneShot(failureSound, Settings.volume);
        }
    }

    public void addLoan(TMP_InputField textField) {
        try {
            float amount = float.Parse(textField.text, CultureInfo.InvariantCulture.NumberFormat);
            if (addDebt(amount, LoanType.Unsecured)) {
                dialogText.text = "All set!";
                audioS.PlayOneShot(paymentSound, Settings.volume);
                textField.text = "";  // clear text field
            }
            else {
                dialogText.text = "Hmm... I would suggest paying off previous debts first.";
                audioS.PlayOneShot(failureSound, Settings.volume);
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning($"{e.Message}");
            dialogText.text = "Perhaps you meant to give me a number?";
            audioS.PlayOneShot(failureSound, Settings.volume);
        }
    }
    public void addSecLoan(TMP_InputField textField) {
        try {
            float amount = float.Parse(textField.text, CultureInfo.InvariantCulture.NumberFormat);
            if (addDebt(amount, LoanType.Secured)) {
                dialogText.text = "YOUR SOUL IS MINE! Erm, I mean... All Set!";
                audioS.PlayOneShot(paymentSound, Settings.volume);
                textField.text = "";  // clear text field
            }
            else {
                dialogText.text = "Nope, declined.";
                audioS.PlayOneShot(failureSound, Settings.volume);
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning($"{e.Message}");
            dialogText.text = "I apologize for the confusion. I can only accept amounts in base ten.";
            audioS.PlayOneShot(failureSound, Settings.volume);
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
                loanEntries[i].amount.text = bankLoans[i].total.ToString("N2");
                loanEntries[i].type.text = $"{(bankLoans[i].source == LoanType.Secured ? $"Secured ({bankLoans[i].collateral.ToString("N2")})" : "Unsecured")}";
            }
        }
    }

    public void BackClick() {
        dialogText.text = "How may I help you?";
        bankPanel.SetActive(false);
        Settings.forceLockCursorState();
        StartCoroutine(StateManager.makePauseAvailableAgain());
    }
}
