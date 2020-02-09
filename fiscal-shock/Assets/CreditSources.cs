using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface Creditor {
    //property signatures
    int maxLoan
    {
        get;
        set;
    }

    int threatLevel
    {
        get;
        set;
    }

    double interestRate
    {
        get;
        set;
    }
}

public class CreditSources : Creditor
{
    //Purpose of this script is to set up initial statistics for creditors, one hostile, one friendly.
    public int maxLoan{get; set;}
    public int threatLevel{get; set;}
    public double interestRate{get; set;}

    public CreditSources(int x, int y, double z) //all values subject to change
    {
        maxLoan = x;
        threatLevel = y;
        interestRate = z;
    }

    public CreditSources bank = new CreditSources(10000, 0, 5.0);
    public CreditSources mob = new CreditSources(4000, 3, 20.3);
}
