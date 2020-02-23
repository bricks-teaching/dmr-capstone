using UnityEngine;

public class SharkScript : MonoBehaviour
{
    //figure out how to import script interfaces
    //Also need threat increase when not paid, gets bad at 5 and really bad at 8
    public static bool sharkDue { get; set; } //This is because the player starts with no debt to the mob

    public bool addDebt(int amount){
        if (PlayerFinance.sharkThreatLevel < 5 && PlayerFinance.sharkMaxLoan > (PlayerFinance.debtShark + amount)){
            //mob threat is below 3 and is below max total debt
            PlayerFinance.debtShark += amount;
            PlayerFinance.cashOnHand += amount;
            sharkDue = true;
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(int amount){
        if (PlayerFinance.cashOnHand < amount){//amount is more than money on hand
            return false;
        } else if (PlayerFinance.debtShark < amount){ //amount is more than the debt
            PlayerFinance.debtShark = 0.0f;//reduce debt to 0 and money on hand by the debt's value
            PlayerFinance.cashOnHand -= PlayerFinance.debtShark;
            sharkDue = false;
            return true;
        } else { //none of the above
            //reduce debt and money by amount
            PlayerFinance.debtShark -= amount;
            PlayerFinance.cashOnHand -= amount;
            sharkDue = false;
            return true;
        }
    }
}
