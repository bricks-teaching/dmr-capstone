using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDay : MonoBehaviour
{
    // to be triggered on arrival from the dungeon, accrues interest and causes hostility if not paid
    public bool startNewDay(bool mobNotPaid, bool bankNotPaid){
        if(mobNotPaid){
            PlayerFinance.setMobThreatLevel(PlayerFinance.getMobThreatLevel() + 1);
        }
        if(bankNotPaid){
            PlayerFinance.setBankThreatLevel(PlayerFinance.getBankThreatLevel() + 1);
        }
        //Increase bank loan by interest rate and reset variable
        PlayerFinance.setDebtBank(PlayerFinance.getDebtBank() + (PlayerFinance.getDebtBank() * PlayerFinance.getBankInterestRate()));
        ATMScript.bankDue = true;
        //Increase Mob loan by interest rate and reset variable if mob debt exists
        if(PlayerFinance.getDebtMob() > 0.0f){
            PlayerFinance.setDebtMob(PlayerFinance.getDebtMob() + (PlayerFinance.getDebtMob() * PlayerFinance.getMobInterestRate()));
            MobsterScript.mobDue = true;
        }

        return true;
    }
}
