using UnityEngine;

public class SharkScript : MonoBehaviour
{
    //figure out how to import script interfaces
    //Also need threat increase when not paid, gets bad at 5 and really bad at 8
    //This is because the player starts with no debt to the shark
    public static bool sharkDue { get; set; } = false;
    public static float sharkInterestRate { get; set; } = 0.155f;
    public static float sharkMaxLoan { get; set; } = 4000.0f;
    public static int sharkThreatLevel { get; set; } = 3;
    public static float sharkTotal { get; set; }

    public bool addDebt(int amount){
        if (sharkThreatLevel < 5 && sharkMaxLoan > (StateManager.totalSharkDebt + amount)){
            //shark threat is below 5 and is below max total debt
            Loan newLoan = new Loan(StateManager.nextID + 1, amount, sharkInterestRate, true);
            StateManager.loanList.AddLast(newLoan);
            PlayerFinance.cashOnHand += amount;
            StateManager.nextID++;
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(int amount, int loanNum){
        if (PlayerFinance.cashOnHand < amount){//amount is more than money on hand
            return false;
        } else if (StateManager.totalSharkDebt <= amount){ //amount is more than the debt
            StateManager.totalSharkDebt = 0.0f;//reduce debt to 0 and money on hand by the debt's value
            PlayerFinance.cashOnHand -= StateManager.totalSharkDebt;
            sharkDue = false;
            StateManager.totalLoans--;
            return true;
        } else { //none of the above
            //reduce debt and money by amount
            StateManager.totalSharkDebt -= amount;
            PlayerFinance.cashOnHand -= amount;
            sharkDue = false;
            return true;
        }
    }

    public static void sharkUnpaid()
    {
        foreach (Loan item in StateManager.loanList)
        {
            if(!item.paid && item.source)
            {
                sharkThreatLevel++;
            }
        }
    }

    public static void sharkInterest()
    {
        float tempTot = 0.0f, tempAdd;
        foreach (Loan item in StateManager.loanList)
        {
            if(item.source)
            {
                item.paid = false;
                tempAdd = item.total * item.rate;
                item.total += tempAdd;
                tempTot += item.total;
            }
        }
        sharkTotal = tempTot;
    }
}
