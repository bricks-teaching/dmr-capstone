using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System;

/// <summary>
/// Runs the loan businesses in the hub, allows for payment and 
/// acquisition of debt. Also manages the GUI text fields.
/// </summary>
public class Creditor : MonoBehaviour {
    public AudioClip paymentSound;
    public AudioClip failureSound;
    private bool playerIsInTriggerZone = false;
    private AudioSource audioS;
    public TextMeshProUGUI dialogText, rateText;
    public GameObject creditorPanel;
    public TMP_InputField paymentId, paymentAmount;
    public List<LoanEntry> loanEntries;
    public List<ValidLoan> validLoans;
    public float maxLoanAmount = 3000f;
    [Tooltip("Lender will not lend loans anymore once their threat level has passed this")]
    public int threatThreshold;
    public string creditorId;
    public int baseThreatLevel;
    public int threatLevel => StateManager.lenders[creditorId].threatLevel;
    public List<Loan> myLoans => StateManager.loanList.Where(l => l.lender == creditorId).ToList();
    public int numberOfLoans => myLoans.Count;
    public float loanTotal => myLoans.Sum(l => l.total);
    /// <summary>
    /// No support for scrolling loan list yet
    /// </summary>
    private readonly int loanHardCap = 3;
    public bool initiallyIndebted;
    public float initialDebtAmount;
    public ValidLoan initialDebtType;
    public float initialRemainingCash;
    private String defaultText;
    /// <summary>
    /// Don't blow out my ears with initial loans as soon as you walk
    /// into the hub
    /// </summary>
    private int debtIsLoud = 1;
    public GameObject tutorial;

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

    /// <summary>
    /// Ran on start. Adds creditors to tracker if not already there,
    ///  adds initial debt and sets the interest rate text for that day.
    /// </summary>
    void Start()
    {
        audioS = GetComponent<AudioSource>();
        defaultText = dialogText.text;
        creditorPanel.SetActive(false);

        // if StateManager isn't already tracking me, add me
        if (!StateManager.lenders.ContainsKey(creditorId)) {
            CreditorData cd = new CreditorData(false, baseThreatLevel);
            StateManager.lenders.Add(creditorId, cd);
        }

        if (!StateManager.sawEntryTutorial && StateManager.cashOnHand <= StateManager.totalDebt && initiallyIndebted) {  // implies this is the first visit to town
            debtIsLoud = 0;
            initialDebtType.addLoanInput.text = initialDebtAmount.ToString();
            addDebt(initialDebtType);
            StateManager.cashOnHand -= initialDebtAmount - initialRemainingCash;
            debtIsLoud = 1;
            dialogText.text = defaultText;
        }
        updateFields();
        Debug.Log($"{creditorId} threat: {threatLevel}, loans: {numberOfLoans}, sum: {loanTotal}");
        //iterator through valid loans that changes loan text in the GUI based on type
        float helperRate, helperCollateral;
        foreach (ValidLoan item in validLoans) {
            if (item.loanType != LoanType.Secured) {
                helperRate = (float)Math.Round(item.interestRate * StateManager.rateAdjuster * 100f, 2);
                item.loanData.text = $"@ {helperRate.ToString("N2")}%";
            } else {
                helperRate = (float)Math.Round(item.interestRate * StateManager.rateAdjuster * item.collateralRateReduction * 100f, 2);
                helperCollateral = (float)Math.Round(item.collateralAmountPercent * 100f, 2);
                item.loanData.text = $"@ {helperRate.ToString("N2")}%\n+ {helperCollateral.ToString("N2")}%";
            }
        }
    }

    /// <summary>
    /// Checks if the player is in range and is pressing the interaction key. 
    /// If so, it will activate the cursor and turn on the menu
    /// </summary>
    void Update()
    {
        if (playerIsInTriggerZone) {
            if (Input.GetKeyDown(Settings.interactKey) && !tutorial.activeSelf) {
                Time.timeScale = 0;
                Settings.forceUnlockCursorState();
                updateFields();
                creditorPanel.SetActive(true);
                StateManager.pauseAvailable = false;
            }
            if (Input.GetKeyDown(Settings.pauseKey)) {
                BackClick();
            }
            if (!Settings.values.sawLoanTutorial) {
                Time.timeScale = 0;
                Settings.forceUnlockCursorState();
                tutorial.SetActive(true);
                Settings.values.sawLoanTutorial = true;
            }
        }
    }

    /// <summary>
    /// Attempts to add a loan of the specified amount of money,
    /// failure conditions included and will affect the dialog
    /// </summary>
    public void addDebt(ValidLoan associatedLoanData) {
        try{
            float amount = float.Parse(associatedLoanData.addLoanInput.text, CultureInfo.InvariantCulture.NumberFormat);
            LoanType loanType = associatedLoanData.loanType;
            if (amount < 0.0f) {
                dialogText.text = associatedLoanData.errorText;
                audioS.PlayOneShot(failureSound, Settings.volume);
                Debug.Log($"{creditorId}: You're supposed to borrow from *me*!");
                return;
            }

            if (
                (threatLevel < threatThreshold)
                && ((maxLoanAmount * StateManager.maxLoanAdjuster) >= (loanTotal + amount))  // secured loans bypass this a bit...
                && (numberOfLoans < loanHardCap)
                ) {
                float modifiedInterest = associatedLoanData.interestRate * associatedLoanData.collateralRateReduction * StateManager.rateAdjuster;  // collateralRateReduction should always be 1 for unsecured loans
                float collateral = (float)Math.Round(associatedLoanData.collateralAmountPercent * amount, 2);  // collateralAmountPercent should always be 0 for unsecured loans
                float modifiedAmount = (float)Math.Round(collateral + amount, 2);
                Loan newLoan = new Loan(StateManager.nextID, modifiedAmount, modifiedInterest, loanType, collateral, creditorId);
                Debug.Log($"{creditorId}: adding ${modifiedAmount} loan with a  {collateral} deposit @ {modifiedInterest*100}%");
                StateManager.loanList.Add(newLoan);
                StateManager.cashOnHand += amount;
                StateManager.nextID++;
                initialDebtType.addLoanInput.text = "";
                updateFields();

                dialogText.text = associatedLoanData.successText;
                audioS.PlayOneShot(paymentSound, Settings.volume * debtIsLoud);
                return;
            }
            Debug.Log($"{creditorId}: Bad dog, no biscuit!");
            dialogText.text = associatedLoanData.failureText;
            audioS.PlayOneShot(failureSound, Settings.volume);
            return;
        }
        catch(Exception e){
            dialogText.text = "Oh, hi, who are you? And why do you smell like motor oil?";
            Debug.Log($"Exception found: {e}");
        }
    }

    /// <summary>
    /// Pays the selected loan by the specified amount.
    /// If the player pays it off the win condition is triggered.
    /// </summary>
    public void payDebt() {
        try{
            float amount = float.Parse(paymentAmount.text, CultureInfo.InvariantCulture.NumberFormat);
            int loanNum = int.Parse(paymentId.text);
            Debug.Log($"{creditorId}: receiving ${amount} payment on loan ${loanNum}");
            Loan selectedLoan = myLoans.First(l => l.ID == loanNum);
            ValidLoan thisLoan = validLoans.First(m => m.loanType == selectedLoan.type);
            // Don't pay off another lender's loans here... Done. -ZM
            if(thisLoan == null){
                dialogText.text = "I think you are looking for the other guy.";
                audioS.PlayOneShot(failureSound, Settings.volume);
                return;
            }
            if (amount < 0.0f || selectedLoan.Equals(null)) {
                dialogText.text = thisLoan.errorPaidText;
                audioS.PlayOneShot(failureSound, Settings.volume);
                return;
            }
            if (StateManager.cashOnHand < amount) { // amount is more than money on hand
                dialogText.text = thisLoan.failurePaidText;
                audioS.PlayOneShot(failureSound, Settings.volume);
                return;
            }
            else if (selectedLoan.total <= amount) { // amount is more than the debt
                StateManager.cashOnHand -= selectedLoan.total;
                StateManager.cashOnHand += selectedLoan.collateral;  // get back extra amount paid on secured loans
                StateManager.loanList.Remove(selectedLoan);
                checkWin();
                updateFields();
                dialogText.text = thisLoan.successPaidText;
                audioS.PlayOneShot(paymentSound, Settings.volume);
                return;
            }
            else { // none of the above
                // reduce debt and money by amount
                selectedLoan.total -= amount;
                StateManager.cashOnHand -= amount;
                updateFields();
                dialogText.text = thisLoan.successPaidText;
                audioS.PlayOneShot(paymentSound, Settings.volume);
                return;
            }
        }
        catch(Exception e){
            dialogText.text = "Wait... What happened? Where am I?";
            Debug.Log($"Exception found: {e}");
        }
    }

    /// <summary>
    /// Updates the GUI so that the player can see their loans.
    /// Allows for accurate information to be displayed.
    /// </summary>
    void updateFields() {
        float tempRate;
        for (int i = 0; i < loanEntries.Count; ++i) {
            if (i >= myLoans.Count) {
                loanEntries[i].id.text = "";
                loanEntries[i].amount.text = "";
                loanEntries[i].type.text = "";
            }
            else {
                tempRate = (float)Math.Round(myLoans[i].rate * 100f, 2);
                loanEntries[i].id.text = myLoans[i].ID.ToString();
                loanEntries[i].amount.text = myLoans[i].total.ToString("N2");
                string typetext = "dummy";
                switch (myLoans[i].type) {
                    case LoanType.Unsecured:
                    case LoanType.Payday:
                        typetext = $"{tempRate.ToString("N2")}%";
                        break;
                    case LoanType.Secured:
                        typetext = $"{tempRate.ToString("N2")}% ({myLoans[i].collateral.ToString("N2")} down)";
                        break;
                }
                loanEntries[i].type.text = typetext;
            }
        }
        rateText.text = $"Max Credit: {(maxLoanAmount * StateManager.maxLoanAdjuster).ToString("N2")}\nTotal Loans: {loanTotal.ToString("N2")}";
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
        Time.timeScale = 1;
        dialogText.text = defaultText;
        tutorial.SetActive(false);
        creditorPanel.SetActive(false);
        Settings.forceLockCursorState();
        StartCoroutine(StateManager.makePauseAvailableAgain());
    }

    public void dismissTutorial() {
        tutorial.SetActive(false);
        Time.timeScale = 1;
    }
}

/// <summary>
/// Loan Entries are helper objects
/// </summary>
[System.Serializable]
public class LoanEntry {
    public TextMeshProUGUI id, type, amount;
}
