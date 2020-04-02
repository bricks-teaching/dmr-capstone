using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;

public class ATMScript : MonoBehaviour {
    public AudioClip paymentSound;
    public AudioClip failureSound;
    public static bool bankDue = true;
    private bool playerIsInTriggerZone = false;
    private int loanCount = 0;
    private AudioSource audioS;
    public GameObject bankPanel;
    public TextMeshProUGUI dialogText;
    public Button payButton;
    public Button newLoan;
    public Button secLoan;
    public Button backButton;
    public InputField payAmount;
    public InputField payID;
    public InputField loanInput;
    public InputField secInput;
    public Text id1, id2, id3, type1, type2, type3, amount1, amount2, amount3;
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
            //signText.text = defaultSignText;
        }
    }

    void Start() {
        //signText = GetComponentInChildren<TextMeshProUGUI>();
        //defaultSignText = signText.text.Replace("INTERACTKEY", Settings.interactKey.ToUpper());
        //signText.text = defaultSignText;
        audioS = GetComponent<AudioSource>();
        bankPanel.SetActive(false);
        Button btnOne = payButton.GetComponent<Button>();
        Button btnTwo = newLoan.GetComponent<Button>();
        Button btnThr = secLoan.GetComponent<Button>();
        Button btnFou = backButton.GetComponent<Button>();
        btnOne.onClick.AddListener(payLoan);
        btnTwo.onClick.AddListener(addLoan);
        btnThr.onClick.AddListener(addSecLoan);
        btnFou.onClick.AddListener(BackClick);
    }

    void Update() {
        if (playerIsInTriggerZone && Input.GetKeyDown(Settings.interactKey)) {
            Settings.forceUnlockCursorState();
            bankPanel.SetActive(true);
            /*bool paymentSuccessful = payDebt(100, 1);
            if (paymentSuccessful) {
                signText.text = "";
                audioS.PlayOneShot(paymentSound, Settings.volume);
                Debug.Log("Paid $100");
            } else {
                signText.text = "Please tender payments using cash, not respects.";
                audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
                Debug.Log("Not enough cash to pay denbts");
            }
            */
        }
    }
    public enum LoanType {
        Unsecured,
        Secured
    }

    public bool addDebt(float amount, LoanType loanType) {
        if(amount < 0.0f){
            return false;
        }
        if (bankThreatLevel < 3 && bankMaxLoan > (bankTotal + amount) && loanCount < 3){
            // bank threat is below 3 and is below max total debt
            Loan newLoan = null;
            switch (loanType) {
                case LoanType.Unsecured:
                    newLoan = new Loan(StateManager.nextID, amount, bankInterestRate, false);
                    bankTotal += amount;
                    break;
                case LoanType.Secured:
                    newLoan = new Loan(StateManager.nextID, amount * securedAmount, bankInterestRate * rateReducer, false);
                    bankTotal += amount * securedAmount;
                    break;
                default:
                    return false; //this shouldnt activate, false return is a failsafe measure
            }
            StateManager.loanList.AddLast(newLoan);
            PlayerFinance.cashOnHand += amount;
            StateManager.nextID++;
            StateManager.totalLoans++;
            StateManager.calcDebtTotals();
            loanCount++;
            updateFields();
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(float amount, int loanNum) {
        Loan selectedLoan = StateManager.loanList.Where(l => l.ID == loanNum).First();
        if(amount < 0.0f || selectedLoan.Equals(null)){
            return false;
        }
        if (PlayerFinance.cashOnHand < amount) { // amount is more than money on hand
            //display a message stating error
            return false;
        } else if (bankTotal <= amount) { // amount is more than the debt
            StateManager.loanList.Remove(selectedLoan);
            PlayerFinance.cashOnHand -= bankTotal;
            bankDue = false;
            StateManager.totalLoans--;
            bankTotal = 0.0f;
            loanCount--;
            StateManager.calcDebtTotals();
            updateFields();
            //temporaryWinGame();
            return true;
        } else { // none of the above
            // reduce debt and money by amount
            selectedLoan.total -= amount;
            PlayerFinance.cashOnHand -= amount;
            bankDue = false;
            bankTotal -= amount;
            StateManager.calcDebtTotals();
            updateFields();
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

    void payLoan(){
        float am = float.Parse(payAmount.text, CultureInfo.InvariantCulture.NumberFormat);
        int az = int.Parse(payID.text);
        bool z = payDebt(am, az);
        if(z){
            dialogText.text = "Thank you for your payment!";
            audioS.PlayOneShot(paymentSound, Settings.volume);
        } else {
            dialogText.text = "You don't have the money on you.";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }
    void addLoan(){
        float an = float.Parse(loanInput.text, CultureInfo.InvariantCulture.NumberFormat);
        bool y = addDebt(an, LoanType.Unsecured);
        if(y){
            dialogText.text = "All set!";
            audioS.PlayOneShot(paymentSound, Settings.volume);
        } else {
            dialogText.text = "Hmm... I would suggest paying off previous debts first.";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }
    void addSecLoan(){
        float ao = float.Parse(secInput.text, CultureInfo.InvariantCulture.NumberFormat);
        bool x = addDebt(ao, LoanType.Secured);
        if(x){
            dialogText.text = "YOUR SOUL IS MINE! Erm, I mean... All Set!";
            audioS.PlayOneShot(paymentSound, Settings.volume);
        } else {
            dialogText.text = "Nope, declined.";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }

    void updateFields(){
        Loan[] item = StateManager.loanList.Where(l => !l.source).ToArray();
        if(item.Length > 0){
        id1.text = item[0].ID.ToString();
        amount1.text = item[0].total.ToString();
        type1.text = "Bank";
        }
        if (item.Length > 1) {
        id2.text = item[1].ID.ToString();
        amount2.text = item[1].total.ToString();
        type2.text = "Bank";
        }
        if(item.Length > 2){
        id3.text = item[2].ID.ToString();
        amount3.text = item[2].total.ToString();
        type3.text = "Bank";
        }
    }

    public void BackClick()
    {
        dialogText.text = "How may I help you?";
        bankPanel.SetActive(false);
        Settings.forceLockCursorState();
    }
}
