using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI pocketChange;
    public TextMeshProUGUI debtTracker;
    // Start is called before the first frame update
    void Start()
    {
        debtTracker.text = "DEBT: -" + (PlayerFinance.debtBank + PlayerFinance.debtShark).ToString("F2");
    }

    // Update is called once per frame
    void Update()
    {
        pocketChange.text = "" + PlayerFinance.cashOnHand.ToString("F2");
    }
}
