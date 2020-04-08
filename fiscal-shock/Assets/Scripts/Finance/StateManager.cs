using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loan
{
    public int ID;// { get; set; }
    public float total;// { get; set; }
    public float rate;// { get; set; }
    public bool paid;// { get; set; }
    public bool source;// { get; set; }
    public int age;// { get; set; }

    //source is true when it is the shark, false when it is the bank
    public Loan(int num, float tot, float rat, bool shark)
    {
        ID = num;
        total = tot;
        rate = rat;
        paid = true;
        source = shark;
        age = 0;
    }
}

public enum DungeonTypeEnum {
    Temple,
    Mine
}

public static class StateManager
{
    //list of loans that the player posesses
    public static LinkedList<Loan> loanList = new LinkedList<Loan>();
    //Total debt of the player updated whenever a loan is drawn out, paid or interest is applied
    //used to calculate average income
    public static LinkedList<float> income = new LinkedList<float>();
    public static float totalDebt { get; set;  }
    public static int nextID { get; set; } = 0;
    public static int totalLoans { get; set; }
    public static int timesEntered { get; set; } = 0;
    public static int currentFloor { get; set; } = 0;
    public static int change { get; set; } = 5;
    public static int creditScore { get; set; }
    public static int paymentStreak { get; set; }
    public static float cashOnEntrance { get; set; }
    public static float averageIncome { get; set; }
    public static bool purchasedHose;
    public static bool purchasedLauncher;

    public static DungeonTypeEnum selectedDungeon { get; set; }
    public static bool sawEntryTutorial = false;
    public static bool inStoryTutorial = false;

    public static void calcCreditScore()
    {
        int baseScore = 500, sharkPen = 0;
        int oldestLoan = 0;
        foreach (Loan item in loanList)
        {
            item.age++;
            if(item.age > oldestLoan){
                oldestLoan = item.age;
            }
            if(item.source){
                sharkPen++;
            }
        }
        if(oldestLoan > 10){
            baseScore -= change * (oldestLoan - 10);
        }
        baseScore -= sharkPen * change;
        if(totalLoans > 5){
            baseScore -= change * 2;
        }
        if(totalDebt > 10000){
            baseScore -= change * 8;
        } else if (totalDebt > 5000){
            baseScore += change * 8;
        }
        baseScore += (paymentStreak * 5);
        if(averageIncome < 0){
            baseScore -= change * 10;
        } else if(averageIncome > totalDebt * 0.03) {
            baseScore += change * 15;
        } else {
            baseScore += change * 5;
        }
    }

    //calculates debt total
    public static void calcDebtTotals()
    {
        totalDebt = SharkScript.sharkTotal + ATMScript.bankTotal;
    }
    public static void calcAverageIncome()
    {
        float tem = 0.0f;
        foreach (float item in StateManager.income)
        {
            tem += item;
        }
        averageIncome = tem / timesEntered;
    }
}
