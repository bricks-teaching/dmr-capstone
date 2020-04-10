using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

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
    public static List<Loan> sharkLoans => StateManager.loanList.Where(l => l.source == LoanType.Payday).ToList();
    public static List<Loan> bankLoans => StateManager.loanList.Where(l => l.source != LoanType.Payday).ToList();
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
            updateFields();
            return true;
        } else if (sharkThreatLevel < 5 && sharkMaxLoan >= (sharkTotal + amount) && sharkLoans.Count < 3 && loanType == LoanType.Payday) {
            //shark threat is below 5 and is below max total debt
            Loan newLoan = new Loan(StateManager.nextID, amount, sharkInterestRate, LoanType.Payday);
            StateManager.loanList.AddLast(newLoan);
            StateManager.cashOnHand += amount;
            StateManager.nextID++;
            updateFields();
            return true;
        } else {
            return false;
        }
    }

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

    public static void isUnpaid(){
        bool paidShark = true;
        bool paidBank = true;
        foreach (Loan item in StateManager.loanList) {
            if (!item.paid) {
                if(item.source == LoanType.Payday){
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

    void updateFields(){
        //not sure how to implement this
    }

    public static void applyInterest(){
        foreach (Loan item in StateManager.loanList) {
            item.paid = false;
            item.total += item.rate * item.total;
        }
    }

    public void checkWin() {
        if (StateManager.loanList.Count == 0) {
            StateManager.playerWon = true;
            GameObject.FindGameObjectWithTag("Loading Screen").GetComponent<LoadingScreen>().startLoadingScreen("WinGame");
        }
    }

    public void BackClick() {
        dialogText.text = ""; //figure this out in a bit
        creditorPanel.SetActive(false);
        Settings.forceLockCursorState();
        StartCoroutine(StateManager.makePauseAvailableAgain());
    }
}

[System.Serializable]
public class LoanEntry {
    public TextMeshProUGUI id, type, amount;
}
