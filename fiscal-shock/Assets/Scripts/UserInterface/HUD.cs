using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI pocketChange;
    public TextMeshProUGUI debtTracker;

    /// <summary>
    /// Currently updates every frame, could also instead just be asked to
    /// update by functions that change the HUD if that is a performance
    /// concern.
    /// </summary>
    void Update() {
        pocketChange.text = "" + PlayerFinance.cashOnHand.ToString("F2");
        debtTracker.text = "DEBT: -" + StateManager.totalDebt.ToString("F2");
    }
}
