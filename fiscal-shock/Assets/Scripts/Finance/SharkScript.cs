using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;

public class SharkScript : MonoBehaviour
{
    public AudioClip paymentSound;
    public AudioClip failureSound;
    public static bool sharkDue { get; set; } = false;
    private bool playerIsInTriggerZone = false;
    private AudioSource audioS;
    public TextMeshProUGUI dialogText;
    public GameObject sharkPanel;
    public Button payButton;
    public Button newLoan;
    public Button backButton;
    public InputField payAmount;
    public InputField payID;
    public InputField loanInput;
    public Text id1, id2, id3, type1, type2, type3, amount1, amount2, amount3;
    public static float sharkInterestRate { get; set; } = 0.155f;
    public static float sharkMaxLoan { get; set; } = 4000.0f;
    public static int sharkThreatLevel { get; set; } = 3;
    public static float sharkTotal { get; set; }
    private int loanCount = 0;

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
        sharkPanel.SetActive(false);
        Button btnOne = payButton.GetComponent<Button>();
        Button btnTwo = newLoan.GetComponent<Button>();
        Button btnThr = backButton.GetComponent<Button>();
        btnOne.onClick.AddListener(payLoan);
        btnTwo.onClick.AddListener(addLoan);
        btnThr.onClick.AddListener(BackClick);
    }

    void Update() {
        if (playerIsInTriggerZone && Input.GetKeyDown(Settings.interactKey)) {
            Settings.forceUnlockCursorState();
            sharkPanel.SetActive(true);
        }
    }
    public bool addDebt(float amount){
        if (sharkThreatLevel < 5 && sharkMaxLoan > (sharkTotal + amount) && loanCount < 3){
            //shark threat is below 5 and is below max total debt
            Loan newLoan = new Loan(StateManager.nextID, amount, sharkInterestRate, true);
            StateManager.loanList.AddLast(newLoan);
            PlayerFinance.cashOnHand += amount;
            StateManager.nextID++;
            StateManager.totalLoans++;
            sharkTotal += amount;
            loanCount++;
            StateManager.calcDebtTotals();
            updateFields();
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(float amount, int loanNum){
        if (PlayerFinance.cashOnHand < amount){//amount is more than money on hand
            return false;
        } else if (sharkTotal <= amount){ //amount is more than the debt
            Loan selectedLoan = StateManager.loanList.Where(l => l.ID == loanNum).First();
            StateManager.loanList.Remove(selectedLoan);
            PlayerFinance.cashOnHand -= sharkTotal;
            sharkDue = false;
            StateManager.totalLoans--;
            sharkTotal = 0.0f;
            loanCount--;
            StateManager.calcDebtTotals();
            updateFields();
            return true;
        } else { //none of the above
            //reduce debt and money by amount
            Loan selectedLoan = StateManager.loanList.Where(l => l.ID == loanNum).First();
            selectedLoan.total -= amount;
            PlayerFinance.cashOnHand -= amount;
            sharkDue = false;
            sharkTotal -= amount;
            StateManager.calcDebtTotals();
            updateFields();
            return true;
        }
    }

    public static void sharkUnpaid()
    {
        bool paid = true;
        foreach (Loan item in StateManager.loanList)
        {
            if(!item.paid && item.source)
            {
                sharkThreatLevel++;
                StateManager.paymentStreak = 0;
                paid = false;
            }
        }
        if(paid){
            StateManager.paymentStreak++;
            sharkThreatLevel--;
        }
    }

    public static void sharkInterest()
    {
        float tempTot = 0.0f;
        float tempAdd;
        foreach (Loan item in StateManager.loanList)
        {
            if(item.source)
            {
                item.paid = false;
                tempAdd = item.total * item.rate;
                item.total += tempAdd;
                tempTot += item.total;
            }
        }
        sharkTotal = tempTot;
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
        bool y = addDebt(an);
        if(y){
            dialogText.text = "I guess I could do you a favor. *snicker*";
            audioS.PlayOneShot(paymentSound, Settings.volume);
        } else {
            dialogText.text = "Do I look like an easy mark to you?";
            audioS.PlayOneShot(failureSound, Settings.volume * 2.5f);
        }
    }

    void updateFields(){
        Loan[] item = StateManager.loanList.Where(l => l.source).ToArray();
        if(item.Length > 0){
            id1.text = item[0].ID.ToString();
            amount1.text = item[0].total.ToString();
            type1.text = "Payday";
        }
        if (item.Length > 1) {
            id2.text = item[1].ID.ToString();
            amount2.text = item[1].total.ToString();
            type2.text = "Payday";
        }
        if(item.Length > 2){
            id3.text = item[2].ID.ToString();
            amount3.text = item[2].total.ToString();
            type3.text = "Payday";
        }
    }

    public void BackClick()
    {
        dialogText.text = "I'll make you an offer you can't refuse.";
        sharkPanel.SetActive(false);
        Settings.forceLockCursorState();
    }
}
