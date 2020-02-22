using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobsterScript : MonoBehaviour
{
    //figure out how to import script interfaces
    //Also need threat increase when not paid, gets bad at 5 and really bad at 8
    public static bool mobDue{get; set;} = false; //This is because the player starts with no debt to the mob

    public bool addDebt(int amount){
        if(PlayerFinance.mobThreatLevel < 5 && PlayerFinance.mobMaxLoan > (PlayerFinance.debtMob + amount)){
            //mob threat is below 3 and is below max total debt
            PlayerFinance.debtMob += amount;
            PlayerFinance.cashOnHand += amount;
            mobDue = true;
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(int amount){
        if(PlayerFinance.cashOnHand < amount){//amount is more than money on hand
            return false;
        } else if(PlayerFinance.debtMob < amount){ //amount is more than the debt
            PlayerFinance.debtMob = 0.0f;//reduce debt to 0 and money on hand by the debt's value
            PlayerFinance.cashOnHand -= PlayerFinance.debtMob;
            mobDue = false;
            return true;
        } else { //none of the above
            //reduce debt and money by amount
            PlayerFinance.debtMob -= amount;
            PlayerFinance.cashOnHand -= amount;
            mobDue = false;
            return true;
        }
    }
}
