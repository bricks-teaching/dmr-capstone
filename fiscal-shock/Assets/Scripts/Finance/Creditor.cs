using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System;

public class Creditor : MonoBehaviour
{
    public AudioClip paymentSound;
    public AudioClip failureSound;
    private bool playerIsInTriggerZone = false;
    private AudioSource audioS;
    public TextMeshProUGUI dialogText, rateText;
    public GameObject creditorPanel;
    public TMP_InputField paymentId, paymentAmount;
    public List<LoanEntry> loanEntries;
    public List<ValidLoan> validLoans;
    public static float sharkMaxLoan { get; set; } = 20000.0f;
    /// <summary>
    /// Bank will loan more based on how many times you've been in dungeon.
    /// Quick fix; can be abused if player pays off loans then just repeatedly
    /// re-enters and the max loan keeps going up. Checks and offsets are
    /// because ln(n < 3) < 1
    /// </summary>
    /// <returns></returns>
    public static float bankMaxLoan => 2600.0f * (StateManager.timesEntered > 0? Mathf.Log(StateManager.timesEntered + 2) : 1);
    public static float sharkInterestRate { get; set; } = 0.3333f;
    public static float bankInterestRate { get; set; } = 0.035f;
    public static int sharkThreatLevel { get; set; } = 3;
    public static int bankThreatLevel { get; set; } = 0;
    public static float securedAmount { get; set; } = 1.15f;
    public static float rateReducer { get; set; } = 0.75f;
    public static List<Loan> sharkLoans => StateManager.loanList.Where(l => l.type == LoanType.Payday).ToList();
    public static List<Loan> bankLoans => StateManager.loanList.Where(l => l.type != LoanType.Payday).ToList();
    public int sharkLoanCount => sharkLoans.Count;
    public int bankLoanCount => bankLoans.Count;
    public static float sharkTotal => sharkLoans.Sum(l => l.total);
    public static float bankTotal => bankLoans.Sum(l => l.total);

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

    // Start is called before the first frame update
    void Start()
    {
        audioS = GetComponent<AudioSource>();
        creditorPanel.SetActive(false);
        if (!StateManager.sawEntryTutorial) {  // implies this is the first visit to town
            addDebt(2000.0f, LoanType.Unsecured);
            StateManager.cashOnHand = 900f;
        }
        updateBankFields();
        updateSharkFields();
        rateText.text = $"Our flexible loan options are always backed by the UDIC.\nUnsecured: {bankInterestRate * 100}%\nSecured: {bankInterestRate * rateReducer * 100}% + {(securedAmount-1)*100}% of amount";
    }

    // Update is called once per frame
    void Update()
    {
        if (playerIsInTriggerZone) {
            if (Input.GetKeyDown(Settings.interactKey)) {
                Settings.forceUnlockCursorState();
                updateBankFields();
                updateSharkFields();
                creditorPanel.SetActive(true);
                StateManager.pauseAvailable = false;
            }
            if (Input.GetKeyDown(Settings.pauseKey)) {
                BackClick();
            }
        }
    }

    /// <summary>
    /// Attempts to add a loan of the specified amount of money, failure conditions
    /// included and will affect the dialog
    /// </summary>
    public bool addDebt(float amount, LoanType loanType) {
        if (amount < 0.0f) {
            return false;
        }
        if (bankThreatLevel < 3 && bankMaxLoan >= (bankTotal + amount) && bankLoans.Count < 3 && loanType != LoanType.Payday){
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
            updateBankFields();
            return true;
        } else if (sharkThreatLevel < 5 && sharkMaxLoan >= (sharkTotal + amount) && sharkLoans.Count < 3 && loanType == LoanType.Payday) {
            //shark threat is below 5 and is below max total debt
            Loan newLoan = new Loan(StateManager.nextID, amount, sharkInterestRate, LoanType.Payday);
            StateManager.loanList.AddLast(newLoan);
            StateManager.cashOnHand += amount;
            StateManager.nextID++;
            updateSharkFields();
            return true;
        } else {
            return false;
        }
    }
    /// <summary>
    /// Pays the selected loan by the specified amount.
    /// If the player pays it off the win condition is triggered.
    /// </summary>
    public bool payDebt(float amount, int loanNum){
        Loan selectedLoan = StateManager.loanList.First(l => l.ID == loanNum);
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
            if(selectedLoan.type != LoanType.Payday){
                updateBankFields();
            } else if(selectedLoan.type == LoanType.Payday){
                updateSharkFields();
            }
            return true;
        }
        else { // none of the above
            // reduce debt and money by amount
            selectedLoan.total -= amount;
            StateManager.cashOnHand -= amount;
            if(selectedLoan.type != LoanType.Payday){
                updateBankFields();
            } else if(selectedLoan.type == LoanType.Payday){
                updateSharkFields();
            }
            return true;
        }
    }
    /// <summary>
    /// Determines if the loans have been paid regularly.
    /// There are consequences to falling behind and slight rewards for keeping up
    /// </summary>
    public static void isUnpaid(){
        bool paidShark = true;
        bool paidBank = true;
        foreach (Loan item in StateManager.loanList) {
            if (!item.paid) {
                if(item.type == LoanType.Payday){
                    sharkThreatLevel++;
                    paidShark = false;
                } else {
                    bankThreatLevel++;
                    paidBank = false;
                }
                StateManager.paymentStreak = 0;
            }
        }
        if (paidShark) {
            StateManager.paymentStreak++;
            sharkThreatLevel--;
        }
        if (paidBank) {
            StateManager.paymentStreak++;
            bankThreatLevel--;
        }
    }

    /// <summary>
    /// Updates data in the Bank's manu
    /// </summary>
    void updateBankFields(){
        for (int i = 0; i < loanEntries.Count; ++i) {
            if (i >= bankLoans.Count) {
                loanEntries[i].id.text = "";
                loanEntries[i].amount.text = "";
                loanEntries[i].type.text = "";
            }
            else {
                loanEntries[i].id.text = bankLoans[i].ID.ToString();
                loanEntries[i].amount.text = bankLoans[i].total.ToString("N2");
                loanEntries[i].type.text = $"{(bankLoans[i].type == LoanType.Secured ? $"Secured ({bankLoans[i].collateral.ToString("N2")})" : "Unsecured")}";
            }
        }
    }

    /// <summary>
    /// updates data in the Shark's menu
    /// </summary>
    void updateSharkFields(){
        for (int i = 0; i < loanEntries.Count; ++i) {
            if (i >= sharkLoans.Count) {
                loanEntries[i].id.text = "";
                loanEntries[i].amount.text = "";
                loanEntries[i].type.text = "";
            }
            else {
                loanEntries[i].id.text = sharkLoans[i].ID.ToString();
                loanEntries[i].amount.text = sharkLoans[i].total.ToString("N2");
                loanEntries[i].type.text = "Payday";
            }
        }
    }

    /// <summary>
    /// Applies interest to every loan in the main list
    /// </summary>
    public static void applyInterest(){
        foreach (Loan item in StateManager.loanList) {
            item.paid = false;
            item.total += (float)Math.Round(item.rate * item.total, 2);
        }
    }

    /// <summary>
    /// Determines if the player has won the game, called after any loan is
    /// fully paid off
    /// </summary>
    public void checkWin() {
        if (StateManager.loanList.Count == 0) {
            StateManager.playerWon = true;
            GameObject.FindGameObjectWithTag("Loading Screen").GetComponent<LoadingScreen>().startLoadingScreen("WinGame");
        }
    }

    /// <summary>
    /// Turns off the panel and allows full movement/pause control
    /// </summary>
    public void BackClick() {
        dialogText.text = ""; //figure this out in a bit
        creditorPanel.SetActive(false);
        Settings.forceLockCursorState();
        StartCoroutine(StateManager.makePauseAvailableAgain());
    }
}

/// <summary>
/// Loan Entries are helper objects 
/// </summary>
[System.Serializable]
public class LoanEntry {
    public TextMeshProUGUI id, type, amount;
}
