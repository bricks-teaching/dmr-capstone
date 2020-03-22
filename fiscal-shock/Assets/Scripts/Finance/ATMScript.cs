using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ATMScript : MonoBehaviour {
    public AudioClip paymentSound;
    public AudioClip failureSound;
    public static bool bankDue = true;
    private bool playerIsInTriggerZone = false;
    private TextMeshProUGUI signText;
    private string defaultSignText;
    private AudioSource audio;
    public static float bankMaxLoan { get; set; } = 10000.0f;
    public static float bankInterestRate { get; set; } = 0.035f;
    public static int bankThreatLevel { get; set; } = 0;
    public static float bankTotal { get; set; }
    public static float securedAmount { get; set; } = 1.15f;
    public static float rateReducer { get; set; } = 0.85f;

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
        if (playerIsInTriggerZone && Input.GetKeyDown("f")) {
            bool paymentSuccessful = payDebt(100, 1);
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
    public enum LoanType {
    Unsecured,
    Secured
    }

    public bool addDebt(float amount, LoanType loanType) {
        //loanType is one for default loan and two for the secured loan
        if (bankThreatLevel < 3 && bankMaxLoan > (bankTotal + amount)){
            // bank threat is below 3 and is below max total debt
            Loan newLoan = null;
            switch (loanType) {
                case LoanType.Unsecured:
                    newLoan = new Loan(StateManager.nextID, amount, bankInterestRate, false);
                    break;
                case LoanType.Secured:
                    newLoan = new Loan(StateManager.nextID, amount * securedAmount, bankInterestRate * rateReducer, false);
                    break;
                default:
                    return false; //this shouldnt activate, false return is a failsafe measure
            }
            StateManager.loanList.AddLast(newLoan);
            PlayerFinance.cashOnHand += amount;
            StateManager.nextID++;
            StateManager.totalLoans++;
            StateManager.calcDebtTotals();
            return true;
        } else {
            return false;
        }

    }

    public bool payDebt(float amount, int loanNum) {
        if (PlayerFinance.cashOnHand < amount) { // amount is more than money on hand
            //display a message stating error
            return false;
        } else if (bankTotal <= amount) { // amount is more than the debt
            Loan selectedLoan = StateManager.loanList.Where(l => l.ID == loanNum).First();
            StateManager.loanList.Remove(selectedLoan);
            PlayerFinance.cashOnHand -= bankTotal;
            bankDue = false;
            StateManager.totalLoans--;
            StateManager.calcDebtTotals();
            temporaryWinGame();
            return true;
        } else { // none of the above
            // reduce debt and money by amount
            Loan selectedLoan = StateManager.loanList.Where(l => l.ID == loanNum).First();
            selectedLoan.total -= amount;
            PlayerFinance.cashOnHand -= amount;
            bankDue = false;
            StateManager.calcDebtTotals();
            return true;
        }
    }

    public static void bankUnpaid()
    {
        bool paid = true;
        foreach (Loan item in StateManager.loanList)
        {
            if(!item.paid && !item.source)
            {
                bankThreatLevel++;
                StateManager.paymentStreak = 0;
                paid = false;
            }
        }
        if(paid){
            StateManager.paymentStreak++;
            bankThreatLevel--;
        }
    }

    public static void bankInterest()
    {
        float tempTot = 0.0f;
        float tempAdd;
        foreach (Loan item in StateManager.loanList)
        {
            if(!item.source)
            {
                item.paid = false;
                tempAdd = item.total * item.rate;
                item.total += tempAdd;
                tempTot += item.total;
            }
        }
        bankTotal = tempTot;
    }

    public void temporaryWinGame() {
        SceneManager.LoadScene("WinGame");
    }
}
