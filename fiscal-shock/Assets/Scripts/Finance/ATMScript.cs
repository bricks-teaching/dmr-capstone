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

    public bool addDebt(float amount) {
        if (bankThreatLevel < 3 && bankMaxLoan > (StateManager.totalBankDebt + amount)){
            // bank threat is below 3 and is below max total debt
            StateManager.totalBankDebt += amount;
            PlayerFinance.cashOnHand += amount;
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(float amount) {
        if (PlayerFinance.cashOnHand < amount) { // amount is more than money on hand
            //display a message stating error
            return false;
        } else if (StateManager.totalBankDebt < amount) { // amount is more than the debt
            StateManager.totalBankDebt = 0.0f; // reduce debt to 0 and money on hand by the debt's value
            PlayerFinance.cashOnHand -= StateManager.totalBankDebt;
            bankDue = false;
            temporaryWinGame();
            return true;
        } else { // none of the above
            // reduce debt and money by amount
            StateManager.totalBankDebt -= amount;
            PlayerFinance.cashOnHand -= amount;
            bankDue = false;
            return true;
        }
    }

    public static void bankUnpaid()
    {

    }

    public static void bankInterest()
    {
        
    }

    public void temporaryWinGame() {
        SceneManager.LoadScene("WinGame");
    }
}
