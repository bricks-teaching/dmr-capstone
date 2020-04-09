using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

public class SharkScript : MonoBehaviour {
    public AudioClip paymentSound;
    public AudioClip failureSound;
    private bool playerIsInTriggerZone = false;
    private AudioSource audioS;
    public TextMeshProUGUI dialogText, rateText;
    public GameObject sharkPanel;
    public TMP_InputField paymentId, paymentAmount;
    public List<LoanEntry> loanEntries;
    public static float sharkMaxLoan { get; set; } = 20000.0f;
    public static float sharkInterestRate { get; set; } = 0.3333f;
    public static int sharkThreatLevel { get; set; } = 3;
    private static List<Loan> sharkLoans => StateManager.loanList.Where(l => l.source == LoanType.Payday).ToList();
    public int loanCount => sharkLoans.Count;
    public static float sharkTotal => sharkLoans.Sum(l => l.total);

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
        updateFields();
        rateText.text = $"Fantastic rates as low as {sharkInterestRate * 100}% available today!";
    }

    void Update() {
        if (playerIsInTriggerZone) {
            if (Input.GetKeyDown(Settings.interactKey)) {
                Settings.forceUnlockCursorState();
                updateFields();
                sharkPanel.SetActive(true);
                StateManager.pauseAvailable = false;
            }
            if (Input.GetKeyDown(Settings.pauseKey)) {
                BackClick();
            }
        }
    }

    public bool addDebt(float amount) {
        if (amount < 0.0f) {
            return false;
        }
        if (sharkThreatLevel < 5 && sharkMaxLoan >= (sharkTotal + amount) && loanCount < 3) {
            //shark threat is below 5 and is below max total debt
            Loan newLoan = new Loan(StateManager.nextID, amount, sharkInterestRate, LoanType.Payday);
            StateManager.loanList.AddLast(newLoan);
            StateManager.cashOnHand += amount;
            StateManager.nextID++;
            updateFields();
            return true;
        }
        return false;
    }

    public bool payDebt(float amount, int loanNum) {
        Loan selectedLoan = sharkLoans.First(l => l.ID == loanNum);
        if (StateManager.cashOnHand < amount) {//amount is more than money on hand
            return false;
        }
        else if (sharkTotal <= amount) { //amount is more than the debt
            StateManager.cashOnHand -= sharkTotal;
            StateManager.loanList.Remove(selectedLoan);
            checkWin();
            updateFields();
            return true;
        }
        else { //none of the above
            //reduce debt and money by amount
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

    public static void sharkUnpaid() {
        bool paid = true;
        foreach (Loan item in sharkLoans) {
            if (!item.paid) {
                sharkThreatLevel++;
                StateManager.paymentStreak = 0;
                paid = false;
            }
        }
        if (paid) {
            StateManager.paymentStreak++;
            sharkThreatLevel--;
        }
    }

    public static void sharkInterest() {
        foreach (Loan item in sharkLoans) {
            item.paid = false;
            item.total += item.rate * item.total;
        }
    }

    public void payLoan() {
        try {
            float am = float.Parse(paymentAmount.text, CultureInfo.InvariantCulture.NumberFormat);
            int az = int.Parse(paymentId.text);
            if (payDebt(am, az)) {
                dialogText.text = "'bout time.";
                audioS.PlayOneShot(paymentSound, Settings.volume);
                paymentAmount.text = "";  // clear text field
                paymentId.text = "";
            }
            else {
                dialogText.text = "Where'd you learn to count, bub?";
                audioS.PlayOneShot(failureSound, Settings.volume);
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning($"{e.Message}");
            dialogText.text = "Don't waste my time with nonsense, kid.";
            audioS.PlayOneShot(failureSound, Settings.volume);
        }
    }
    public void addLoan(TMP_InputField textField) {
        try {
            float an = float.Parse(textField.text, CultureInfo.InvariantCulture.NumberFormat);
            if (addDebt(an)) {
                dialogText.text = "I guess I could do you a favor. *snicker*";
                audioS.PlayOneShot(paymentSound, Settings.volume);
                textField.text = "";  // clear text field
            }
            else {
                dialogText.text = "Do I look like an easy mark to you?";
                audioS.PlayOneShot(failureSound, Settings.volume);
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning($"{e.Message}");
            dialogText.text = "You some kinda wiseguy?";
            audioS.PlayOneShot(failureSound, Settings.volume);
        }
    }

    void updateFields() {
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

    public void BackClick() {
        dialogText.text = "I'll make you an offer you can't refuse.";
        sharkPanel.SetActive(false);
        Settings.forceLockCursorState();
        StartCoroutine(StateManager.makePauseAvailableAgain());
    }
}
