using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loan
{
    float total {get; set;}
    float rate {get; set;}
    bool paid {get; set;}
    bool source {get; set;}
    int age {get; set;}

    //source is true when it is the shark, false when it is the bank
    public void loan(float tot, float rat, bool yes, bool shark)
    {
        total = tot;
        rate = rat;
        paid = yes;
        source = shark;
        age = 0;
    }
}

public class StateManager : MonoBehaviour
{
    //list of loans that the player posesses
    public LinkedList<Loan> loanList = new LinkedList<Loan>();
    //Total debt of the player updated whenever a loan is drawn out, paid or interest is applied
    public static float totalDebt {get; set;}
    public static float totalBankDebt {get; set;}
    public static float totalSharkDebt {get; set;}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void calcCreditScore()
    {

    }

    //calculates debt total
    public void calcDebtTotals()
    {

    }
}
