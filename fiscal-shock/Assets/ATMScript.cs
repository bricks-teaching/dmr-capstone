using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATMScript : MonoBehaviour
{
    public static bool bankDue = true;

    //figure out how to import script interfaces
    public bool addDebt(float amount){
        if(PlayerFinance.getBankThreatLevel() < 3 && PlayerFinance.getBankMaxLoan() > (PlayerFinance.getDebtBank() + amount)){
            //bank threat is below 3 and is below max total debt
            PlayerFinance.setDebtBank(PlayerFinance.getDebtBank() + amount);
            PlayerFinance.setCashOnHand(PlayerFinance.getCashOnHand() + amount);
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(float amount){
        if(PlayerFinance.getCashOnHand() < amount){//amount is more than money on hand
            //display a message stating error
            return false;
        } else if(PlayerFinance.getDebtBank() < amount){ //amount is more than the debt
            PlayerFinance.setCashOnHand(PlayerFinance.getCashOnHand() - PlayerFinance.getDebtBank());
            PlayerFinance.setDebtBank(0.0f);//reduce debt to 0 and money on hand by the debt's value
            bankDue = false;
            return true;
        } else { //none of the above
            //reduce debt and money by amount
            PlayerFinance.setDebtBank(PlayerFinance.getDebtBank() - amount);
            PlayerFinance.setCashOnHand(PlayerFinance.getCashOnHand() - amount);
            bankDue = false;
            return true;
        }
    }
}
