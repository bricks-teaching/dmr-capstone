using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface Creditor {
    //property signatures
    public float maxLoan
    {
        get;
    }

    public int threatLevel
    {
        get;
    }

    public double interestRate
    {
        get;
    }
}

public class CreditSources : Creditor
{
    //Purpose of this script is to set up initial statistics for creditors, one hostile, one friendly.
    public float maxLoan{get;}
    public int threatLevel{get;}
    public double interestRate{get;}

    public CreditSources(float loanMax, int threat, double interest) //all values subject to change
    {
        maxLoan = loanMax;
        threatLevel = threat;
        interestRate = interest;
    }

    public CreditSources bank = new CreditSources(10000.0f, 0, 5.0);
    public CreditSources mob = new CreditSources(4000.0f, 3, 20.3);
}
