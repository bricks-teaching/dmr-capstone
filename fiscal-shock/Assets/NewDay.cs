using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDay : MonoBehaviour
{
    // to be triggered on arrival from the dungeon, accrues interest and causes hostility if not paid
    public bool startNewDay(bool mobNotPaid, bool bankNotPaid){
        if(mobNotPaid){
            PlayerFinance.mobThreatLevel++;
        }
        if(bankNotPaid){
            PlayerFinance.bankThreatLevel++;
        }
        //Increase bank loan by interest rate and reset variable
        PlayerFinance.debtBank += PlayerFinance.debtBank * PlayerFinance.bankInterestRate;
        ATMScript.bankDue = true;
        //Increase Mob loan by interest rate and reset variable if mob debt exists
        if(PlayerFinance.debtMob > 0.0f){
            PlayerFinance.debtMob += PlayerFinance.debtMob * PlayerFinance.mobInterestRate;
            MobsterScript.mobDue = true;
        }
        return true;
    }
}
