using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            Debug.Log($"{gameObject.name}: Triggered");
            playerIsInTriggerZone = true;
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Player") {
            Debug.Log($"{gameObject.name}: Left ATM");
            playerIsInTriggerZone = false;
            signText.text = defaultSignText;
        }
    }

    void Start() {
        signText = GetComponentInChildren<TextMeshProUGUI>();
        defaultSignText = signText.text;
        audio = GetComponent<AudioSource>();
    }

    void Update() {
        if (playerIsInTriggerZone && Input.GetKeyDown("f")) {
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

    public bool addDebt(float amount, int loanType) {
        //loanType is one for default loan and two for the secured loan
        if (bankThreatLevel < 3 && bankMaxLoan > (bankTotal + amount)){
            // bank threat is below 3 and is below max total debt
            Loan newLoan = null;
            if(loanType == 1){ //basic loan
                newLoan = new Loan(StateManager.nextID, amount, bankInterestRate, false);
            } else if (loanType == 2){ //secured loan
                newLoan = new Loan(StateManager.nextID, amount * 1.15f, bankInterestRate * 0.85f, false);
            } else {return false;} //this shouldnt activate, false return is a failsafe measure
            StateManager.loanList.AddLast(newLoan);
            PlayerFinance.cashOnHand += amount;
            StateManager.nextID++;
            StateManager.totalLoans++;
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
            foreach (Loan item in StateManager.loanList)
            {
                if(item.ID == loanNum){
                    StateManager.loanList.Remove(item); //reduce debt to 0 and money on hand by the debt's value
                }
            }
            PlayerFinance.cashOnHand -= bankTotal;
            bankDue = false;
            StateManager.totalLoans--;
            temporaryWinGame();
            return true;
        } else { // none of the above
            // reduce debt and money by amount
            foreach (Loan item in StateManager.loanList)
            {
                if(item.ID == loanNum){
                    item.total -= amount;
                }
            }
            PlayerFinance.cashOnHand -= amount;
            bankDue = false;
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
        }
    }

    public static void bankInterest()
    {
        float tempTot = 0.0f, tempAdd;
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
