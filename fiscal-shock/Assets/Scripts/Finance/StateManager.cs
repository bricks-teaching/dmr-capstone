using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loan
{
    float total {get; set;}
    float rate {get; set;}
    bool paid {get; set;}
    bool source {get; set;}

    //source is true when it is the shark, false when it is the bank
    public void loan(float tot, float rat, bool yes, bool shark)
    {
        total = tot;
        rate = rat;
        paid = yes;
        source = shark;
    }
}

public class StateManager : MonoBehaviour
{
    //list of loans that the player posesses
    public LinkedList<Loan> loanList = new LinkedList<Loan>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void calcCreditScore()
    {

    }

}
