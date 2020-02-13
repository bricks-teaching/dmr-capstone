//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public static class PlayerFinance
{
    private static float cashOnHand = 1000.0f;
    private static float debtBank = 2500.0f;
    private static float bankMaxLoan = 10000.0f;
    private static float bankInterestRate = 0.035f;
    private static int bankThreatLevel = 0;
    private static float debtMob = 0.0f;
    private static float mobMaxLoan = 4000.0f;
    private static float mobInterestRate = 0.155f;
    private static int mobThreatLevel = 3;

    public static void setCashOnHand(float cash){
        cashOnHand = cash;
    }

    public static float getCashOnHand(){
        return cashOnHand;
    }

    public static void setDebtBank(float debtB){
        debtBank = debtB;
    }

    public static float getDebtBank(){
        return debtBank;
    }

    public static void setBankMaxLoan(float maxB){
        bankMaxLoan = maxB;
    }

    public static float getBankMaxLoan(){
        return bankMaxLoan;
    }

    public static void setBankInterestRate(float bankInterest){
        bankInterestRate = bankInterest;
    }

    public static float getBankInterestRate(){
        return bankInterestRate;
    }

    public static void setBankThreatLevel(int bankThreat){
        bankThreatLevel = bankThreat;
    }

    public static int getBankThreatLevel(){
        return bankThreatLevel;
    }

    public static void setDebtMob(float debtM){
        debtMob = debtM;
    }

    public static float getDebtMob(){
        return debtMob;
    }

    public static void setMobMaxLoan(float maxM){
        mobMaxLoan = maxM;
    }

    public static float getMobMaxLoan(){
        return mobMaxLoan;
    }

    public static void setMobInterestRate(float mobInterest){
        mobInterestRate = mobInterest;
    }

    public static float getMobInterestRate(){
        return mobInterestRate;
    }

    public static void setMobThreatLevel(int mobThreat){
        mobThreatLevel = mobThreat;
    }

    public static int getMobThreatLevel(){
        return mobThreatLevel;
    }
}
