using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Loan
{
    public int ID;// { get; set; }
    public float total;// { get; set; }
    public float rate;// { get; set; }
    public bool paid;// { get; set; }
    public LoanType source;// { get; set; }
    public int age;// { get; set; }

    public Loan(int num, float tot, float rat, LoanType type)
    {
        ID = num;
        total = tot;
        rate = rat;
        paid = true;
        source = type;
        age = 0;
    }
}

public enum LoanType {
    Payday,
    Unsecured,
    Secured
}

public enum DungeonTypeEnum {
    Temple,
    Mine
}

public static class StateManager
{
    public static float cashOnHand { get; set; } = DefaultState.cashOnHand;
    //list of loans that the player posesses
    public static LinkedList<Loan> loanList = new LinkedList<Loan>();
    //Total debt of the player updated whenever a loan is drawn out, paid or interest is applied
    //used to calculate average income
    public static LinkedList<float> income = new LinkedList<float>();
    public static float totalDebt => loanList.Sum(l => l.total);
    public static int nextID { get; set; } = DefaultState.nextID;
    public static int totalLoans => loanList.Count;
    public static int timesEntered { get; set; } = DefaultState.timesEntered;
    public static int currentFloor { get; set; } = DefaultState.currentFloor;
    public static int change { get; set; } = DefaultState.change;
    public static int creditScore { get; set; } = DefaultState.creditScore;
    public static int paymentStreak { get; set; } = DefaultState.paymentStreak;
    public static float cashOnEntrance { get; set; } = DefaultState.cashOnEntrance;
    public static float averageIncome { get; set; } = DefaultState.averageIncome;
    public static bool purchasedHose = DefaultState.purchasedHose;
    public static bool purchasedLauncher = DefaultState.purchasedLauncher;

    public static DungeonTypeEnum selectedDungeon { get; set; }
    public static bool sawTutorial = false;

    public static List<GameObject> singletons = new List<GameObject>();
    public static bool pauseAvailable = true;

    public static bool startNewDay() {
        Debug.Log($"Accumulating interest for day {StateManager.timesEntered}");

        //If unpaid debts present up threat level
        SharkScript.sharkUnpaid();
        //activates interest method in sharkscript also sets paid to false
        SharkScript.sharkInterest();
        //If unpaid debts present up threat level
        ATMScript.bankUnpaid();
        //activates interest method in atmscript also sets paid to false
        ATMScript.bankInterest();

        income.AddLast(cashOnHand - cashOnEntrance);
        calcAverageIncome();
        calcCreditScore();
        Debug.Log($"New debt total: {totalDebt}");
        return true;
    }

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
            if(item.source == LoanType.Payday){
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

    public static void calcAverageIncome()
    {
        float tem = 0.0f;
        foreach (float item in StateManager.income)
        {
            tem += item;
        }
        averageIncome = tem / timesEntered;
    }

    public static void resetToDefaultState() {
        cashOnHand = DefaultState.cashOnHand;
        loanList.Clear();
        income.Clear();
        nextID = DefaultState.nextID;
        timesEntered = DefaultState.timesEntered;
        currentFloor = DefaultState.currentFloor;
        change = DefaultState.change;
        creditScore = DefaultState.creditScore;
        paymentStreak = DefaultState.paymentStreak;
        cashOnEntrance = DefaultState.cashOnEntrance;
        averageIncome = DefaultState.averageIncome;
        purchasedHose = DefaultState.purchasedHose;
        purchasedLauncher = DefaultState.purchasedLauncher;
        sawTutorial = DefaultState.sawTutorial;
        singletons.Clear();
        pauseAvailable = DefaultState.pauseAvailable;
    }
}

/// <summary>
/// Value types only!
/// Reference types (lists, gameobjects, etc.) must be cleared inside
/// the reset function.
/// </summary>
public static class DefaultState {
    public readonly static float cashOnHand = 0.0f;
    public readonly static int nextID = 0;
    public readonly static int timesEntered = 0;
    public readonly static int currentFloor = 0;
    public readonly static int change = 5;
    public readonly static int creditScore = 0;
    public readonly static int paymentStreak = 0;
    public readonly static float cashOnEntrance = 0;
    public readonly static float averageIncome = 0;
    public readonly static bool purchasedHose = false;
    public readonly static bool purchasedLauncher = false;
    public readonly static bool sawTutorial = false;
    public readonly static bool pauseAvailable = true;
}
