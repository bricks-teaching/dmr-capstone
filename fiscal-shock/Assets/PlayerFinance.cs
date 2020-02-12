//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

interface PlayerF {
    //property signatures
    public float cashOnHand
    {
        get;
        set;
    }

    public float debtBank
    {
        get;
        set;
    }

    public float debtMob
    {
        get;
        set;
    }
}

public class PlayerFinance : PlayerF
{
    public float cashOnHand{get; set;}
    public float debtBank{get; set;}
    public float debtMob{get; set;}
    // Start is called before the first frame update
    public PlayerFinance(float cash, float bankDebt, float mobDebt)
    {
        cashOnHand = cash;
        debtBank = bankDebt;
        debtMob = mobDebt;
    }

    public PlayerFinance finances = new PlayerFinance(0.0f, 1000.0f, 0.0f);
}
