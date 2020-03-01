using UnityEngine;

public class NewDay : MonoBehaviour {
    // to be triggered on arrival from the dungeon, accrues interest and causes hostility if not paid
    public bool startNewDay(bool mobNotPaid, bool bankNotPaid){
        if (mobNotPaid){
            PlayerFinance.sharkThreatLevel++;
        }
        if (bankNotPaid){
            PlayerFinance.bankThreatLevel++;
        }
        //Increase bank loan by interest rate and reset variable
        PlayerFinance.debtBank += PlayerFinance.debtBank * PlayerFinance.bankInterestRate;
        ATMScript.bankDue = true;
        //Increase Mob loan by interest rate and reset variable if Shark debt exists
        if (PlayerFinance.debtShark > 0.0f){
            PlayerFinance.debtShark += PlayerFinance.debtShark * PlayerFinance.sharkInterestRate;
            SharkScript.sharkDue = true;
        }
        return true;
    }
}
