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
    //public static float sharkMaxLoan { get; set; } = 20000.0f;
    public float maxLoanAmount = 3000f;
    /// <summary>
    /// Bank will loan more based on how many times you've been in dungeon.
    /// Quick fix; can be abused if player pays off loans then just repeatedly
    /// re-enters and the max loan keeps going up. Checks and offsets are
    /// because ln(n < 3) < 1
    /// </summary>
    /// <returns></returns>
    //public static float bankMaxLoan => 2600.0f * (StateManager.timesEntered > 0? Mathf.Log(StateManager.timesEntered + 2) : 1);
    //public static float sharkInterestRate { get; set; } = 0.3333f;
    //public static float bankInterestRate { get; set; } = 0.035f;
    //public static int sharkThreatLevel { get; set; } = 3;
    //public static int bankThreatLevel { get; set; } = 0;
    [Tooltip("Lender will not lend loans anymore once their threat level has passed this")]
    public int threatThreshold;
    //public static float securedAmount { get; set; } = 1.15f;
    //public static float rateReducer { get; set; } = 0.75f;
    public string creditorId;
    public int baseThreatLevel;
    public int threatLevel => StateManager.lenders[creditorId].threatLevel;
    public List<Loan> myLoans => StateManager.loanList.Where(l => l.lender == creditorId).ToList();
    //public static List<Loan> sharkLoans => StateManager.loanList.Where(l => l.type == LoanType.Payday).ToList();
    //public static List<Loan> bankLoans => StateManager.loanList.Where(l => l.type != LoanType.Payday).ToList();
    public int numberOfLoans => myLoans.Count;
    //public int sharkLoanCount => sharkLoans.Count;
    //public int bankLoanCount => bankLoans.Count;
    public float loanTotal => myLoans.Sum(l => l.total);
    //public static float sharkTotal => sharkLoans.Sum(l => l.total);
    //public static float bankTotal => bankLoans.Sum(l => l.total);
    /// <summary>
    /// No support for scrolling loan list yet
    /// </summary>
    private readonly int loanHardCap = 3;
    public bool initiallyIndebted;
    public float initialDebtAmount;
    public ValidLoan initialDebtType;
    public float initialRemainingCash;

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

        // if StateManager isn't already tracking me, add me
        if (!StateManager.lenders.ContainsKey(creditorId)) {
            CreditorData cd = new CreditorData {
                paid = false,
                threatLevel = baseThreatLevel
            };
            StateManager.lenders.Add(creditorId, cd);
        }

        if (!StateManager.sawEntryTutorial && StateManager.cashOnHand <= StateManager.totalDebt && initiallyIndebted) {  // implies this is the first visit to town
            initialDebtType.addLoanInput.text = initialDebtAmount.ToString();
            addDebt(initialDebtType);
            StateManager.cashOnHand -= initialDebtAmount - initialRemainingCash;
        }
        updateFields();

        Debug.Log($"{creditorId} threat: {threatLevel}, loans: {numberOfLoans}, sum: {loanTotal}");
        //updateBankFields();
        //updateSharkFields();
        /* TODO:
            - Add a public field for dialog above the rate listing so different creditors can have different personalities for this part
            - Edit ratetext to show loan types vs interest rates offered, just a simple listing. You need to loop over valid loans and append the appropriate text and values
         */
        //rateText.text = $"Our flexible loan options are always backed by the UDIC.\nUnsecured: {bankInterestRate * 100}%\nSecured: {bankInterestRate * rateReducer * 100}% + {(securedAmount-1)*100}% of amount";
    }

    // Update is called once per frame
    void Update()
    {
        if (playerIsInTriggerZone) {
            if (Input.GetKeyDown(Settings.interactKey)) {
                Settings.forceUnlockCursorState();
                updateFields();
                //updateBankFields();
                //updateSharkFields();
                creditorPanel.SetActive(true);
                StateManager.pauseAvailable = false;
            }
            if (Input.GetKeyDown(Settings.pauseKey)) {
                BackClick();
            }
        }
    }

    public bool stupid(string s) {
        return false;
    }

    /// <summary>
    /// Attempts to add a loan of the specified amount of money, failure conditions
    /// included and will affect the dialog
    /// </summary>
    public void addDebt(ValidLoan associatedLoanData) {
        // need try-catch here

        float amount = float.Parse(associatedLoanData.addLoanInput.text, CultureInfo.InvariantCulture.NumberFormat);
        LoanType loanType = associatedLoanData.loanType;
        if (amount < 0.0f) {
            // update text field
            Debug.Log($"{creditorId}: You're supposed to borrow from *me*!");
            return;
        }
        //if (bankThreatLevel < 3 && bankMaxLoan >= (bankTotal + amount) && bankLoans.Count < 3 && loanType != LoanType.Payday){

        if (
            (threatLevel < threatThreshold)
            && (maxLoanAmount >= (loanTotal + amount))  // secured loans bypass this a bit...
            && (numberOfLoans < loanHardCap)
            ) {

            float modifiedInterest = associatedLoanData.interestRate * associatedLoanData.collateralRateReduction;  // collateralRateReduction should always be 1 for unsecured loans
            float collateral = (float)Math.Round(associatedLoanData.collateralAmountPercent * amount, 2);  // collateralAmountPercent should always be 0 for unsecured loans
            float modifiedAmount = (float)Math.Round(collateral + amount, 2);
            Loan newLoan = new Loan(StateManager.nextID, modifiedAmount, modifiedInterest, loanType, collateral, creditorId);
            Debug.Log($"{creditorId}: adding ${modifiedAmount} loan with a  {collateral} deposit @ {modifiedInterest*100}%");

            /*
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
            */
            StateManager.loanList.AddLast(newLoan);
            StateManager.cashOnHand += amount;
            StateManager.nextID++;
            initialDebtType.addLoanInput.text = "";
            //updateBankFields();
            updateFields();

            // update tex fiel
            // ALSO ADD SOUNDS WHENEVER THER EIS TEXT HAPPENING
            // audioS.PlayOneShot(clipName, Settings.volume)
            return;
        }
        /* else if (sharkThreatLevel < 5 && sharkMaxLoan >= (sharkTotal + amount) && sharkLoans.Count < 3 && loanType == LoanType.Payday) {
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
        */
        Debug.Log($"{creditorId}: Bad dog, no biscuit!");
        // update tex field??
        return;
    }

    /// <summary>
    /// Pays the selected loan by the specified amount.
    /// If the player pays it off the win condition is triggered.
    /// </summary>
    public void payDebt() {
        // need try-catch here

        float amount = float.Parse(paymentAmount.text, CultureInfo.InvariantCulture.NumberFormat);
        int loanNum = int.Parse(paymentId.text);
        Debug.Log($"{creditorId}: receiving ${amount} payment on loan ${loanNum}");
        // Don't pay off another lender's loans here...
        Loan selectedLoan = myLoans.First(l => l.ID == loanNum);
        if (amount < 0.0f || selectedLoan.Equals(null)) {
            // text here
            return;
        }
        if (StateManager.cashOnHand < amount) { // amount is more than money on hand
            //display a message stating error
            return;
        }
        else if (selectedLoan.total <= amount) { // amount is more than the debt
            StateManager.cashOnHand -= selectedLoan.total;
            StateManager.cashOnHand += selectedLoan.collateral;  // get back extra amount paid on secured loans
            StateManager.loanList.Remove(selectedLoan);
            checkWin();
            updateFields();
            /*
            if(selectedLoan.type != LoanType.Payday){
                updateBankFields();
            } else if(selectedLoan.type == LoanType.Payday){
                updateSharkFields();
            }
            */
            // text yo
            return;
        }
        else { // none of the above
            // reduce debt and money by amount
            selectedLoan.total -= amount;
            StateManager.cashOnHand -= amount;
            updateFields();
            /*
            if(selectedLoan.type != LoanType.Payday){
                updateBankFields();
            } else if(selectedLoan.type == LoanType.Payday){
                updateSharkFields();
            }
            */
            // stuff
            return;
        }
    }

    void updateFields() {
        //Debug.Log($"{creditorId}: updating {numberOfLoans} loans, total: {loanTotal}");
        for (int i = 0; i < loanEntries.Count; ++i) {
            if (i >= myLoans.Count) {
                loanEntries[i].id.text = "";
                loanEntries[i].amount.text = "";
                loanEntries[i].type.text = "";
            }
            else {
                loanEntries[i].id.text = myLoans[i].ID.ToString();
                loanEntries[i].amount.text = myLoans[i].total.ToString("N2");
                string typetext = "dummy";
                switch (myLoans[i].type) {
                    case LoanType.Unsecured:
                        typetext = "Unsecured";
                        break;
                    case LoanType.Payday:
                        typetext = "Payday";
                        break;
                    case LoanType.Secured:
                        typetext = $"Secured ({myLoans[i].collateral.ToString("N2")})";
                        break;
                }
                loanEntries[i].type.text = typetext;
            }
        }
    }

/*
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
    */

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
