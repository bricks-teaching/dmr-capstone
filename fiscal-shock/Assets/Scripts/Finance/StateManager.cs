using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loan
{
    public int ID {get; set;}
    public float total {get; set;}
    public float rate {get; set;}
    public bool paid {get; set;}
    public bool source {get; set;}
    public int age {get; set;}

    //source is true when it is the shark, false when it is the bank
    public void Lsoan(int num, float tot, float rat, bool shark)
    {
        ID = num;
        total = tot;
        rate = rat;
        paid = true;
        source = shark;
        age = 0;
    }
}

public class StateManager : MonoBehaviour
{
    //list of loans that the player posesses
    public static LinkedList<Loan> loanList = new LinkedList<Loan>();
    //Total debt of the player updated whenever a loan is drawn out, paid or interest is applied
    public static float totalDebt {get; set;}
    public static int nextID {get; set;}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static void calcCreditScore()
    {

    }

    //calculates debt total
    public static void calcDebtTotals()
    {
        totalDebt = SharkScript.sharkTotal + ATMScript.bankTotal;
    }
}
