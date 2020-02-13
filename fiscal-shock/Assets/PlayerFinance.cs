//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class PlayerFinance
{
    public static float cashOnHand{get; set;}
    public static float debtBank{get; set;}
    public static float bankMaxLoan{get; set;}
    public static int bankThreatLevel{get; set;}
    public static float bankInterestRate{get; set;}
    public static float debtMob{get; set;}
    public static float mobMaxLoan{get; set;}
    public static int mobThreatLevel{get; set;}
    public static float mobInterestRate{get; set;}

    public PlayerFinance(){
        cashOnHand = 1000.0f;
        debtBank = 2500.0f;
        bankMaxLoan = 10000.0f;
        bankThreatLevel = 0;
        bankInterestRate = 3.5f;
        debtMob = 0.0f;
        mobMaxLoan = 4000.0f;
        mobThreatLevel = 3;
        mobInterestRate = 15.5f;
    }
}
