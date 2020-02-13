//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public static class PlayerFinance
{
    public static float cashOnHand{get; set} = 1000.0f;
    public static float debtBank{get; set} = 2500.0f;
    private static float bankMaxLoan{get; set} = 10000.0f;
    private static float bankInterestRate{get; set} = 0.035f;
    private static int bankThreatLevel{get; set} = 0;
    private static float debtMob{get; set} = 0.0f;
    private static float mobMaxLoan{get; set} = 4000.0f;
    private static float mobInterestRate{get; set} = 0.155f;
    private static int mobThreatLevel{get; set} = 3;

}
