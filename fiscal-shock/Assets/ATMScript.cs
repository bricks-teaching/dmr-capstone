using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATMScript : MonoBehaviour
{
    public static bool bankDue = true;

    //figure out how to import script interfaces
    public bool addDebt(float amount){
        if(PlayerFinance.bankThreatLevel < 3 && PlayerFinance.bankMaxLoan > (PlayerFinance.debtBank + amount)){
            //bank threat is below 3 and is below max total debt
            PlayerFinance.debtBank += amount;
            PlayerFinance.cashOnHand += amount;
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(float amount){
        if(PlayerFinance.cashOnHand.CompareTo(amount) < 0){//amount is more than money on hand
            //display a message stating error
            return false;
        } else if(PlayerFinance.debtBank.CompareTo(amount) < 0){ //amount is more than the debt
            PlayerFinance.debtBank = 0.0f;//reduce debt to 0 and money on hand by the debt's value
            PlayerFinance.cashOnHand -= PlayerFinance.debtBank;
            bankDue = false;
            return true;
        } else { //none of the above
            //reduce debt and money by amount
            PlayerFinance.debtBank -= amount;
            PlayerFinance.cashOnHand -= amount;
            bankDue = false;
            return true;
        }
    }
}
