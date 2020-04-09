using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// Represents a single loan.
/// </summary>
public class Loan
{
    public int ID { get; set; }
    public float total { get; set; }
    public float rate { get; set; }
    public bool paid { get; set; }
    public LoanType source { get; set; }
    public int age { get; set; }
    public float originalAmount { get; private set; }
    public float collateral => (source == LoanType.Secured)? originalAmount - (originalAmount / ATMScript.securedAmount) : 0;

    public Loan(int num, float tot, float rat, LoanType type)
    {
        ID = num;
        total = tot;
        rate = rat;
        paid = true;
        source = type;
        age = 0;
        originalAmount = tot;
    }
}

/// <summary>
/// Valid loan types. LoanType.Payday implies that a loan is from
/// the Loan Shark and not the Bank.
/// </summary>
public enum LoanType {
    Payday,
    Unsecured,
    Secured
}

/// <summary>
/// Valid dungeon types. Used to communicate to the Dungeoneer
/// what kind of dungeon to load.
/// </summary>
public enum DungeonTypeEnum {
    Temple,
    Mine
}

/// <summary>
/// Represents a play session. Consider this the player's
/// "save data." Save games could be implemented by serializing
/// this to JSON (may require a separate backing data class with
/// no methods).
/// </summary>
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
    public static float averageIncome => income.Average();
    public static bool purchasedHose = DefaultState.purchasedHose;
    public static bool purchasedLauncher = DefaultState.purchasedLauncher;

    public static DungeonTypeEnum selectedDungeon { get; set; }
    public static bool sawEntryTutorial = false;
    public static bool inStoryTutorial = false;

    public static List<GameObject> singletons = new List<GameObject>();
    public static bool pauseAvailable = true;
    public static bool playerDead = false;
    public static bool playerWon = false;

    /// <summary>
    /// Hitting "esc" to exit GUIs sometimes hits the pause code too,
    /// depending on order of execution. Bad things happen when the pause menu
    /// has a different order of execution. So, this is the nicest way to
    /// avoid bringing up the pause menu when someone exits a shop via
    /// keyboard.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator makePauseAvailableAgain() {
        yield return new WaitForSeconds(0.5f);
        StateManager.pauseAvailable = true;
        yield return null;
    }

    /// <summary>
    /// Calls creditor functions to accumulate interest and gets income earned
    /// from the last dungeon dive.
    /// </summary>
    /// <returns></returns>
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
        calcCreditScore();
        Debug.Log($"New debt total: {totalDebt}");
        return true;
    }

    /// <summary>
    /// Calculates the player's credit score. Not currently used.
    /// </summary>
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

    /// <summary>
    /// Resets the StateManager to the default state. Use this when
    /// the player's session needs to be reset. The StateManger is
    /// set up for a "fresh" playthrough.
    /// </summary>
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
        purchasedHose = DefaultState.purchasedHose;
        purchasedLauncher = DefaultState.purchasedLauncher;
        sawEntryTutorial = DefaultState.sawTutorial;
        singletons.Clear();
        pauseAvailable = DefaultState.pauseAvailable;
        playerDead = DefaultState.playerDead;
        playerWon = DefaultState.playerWon;
    }
}

/// <summary>
/// Default StateManager values.
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
    public readonly static float cashOnEntrance = 0.0f;
    public readonly static bool purchasedHose = false;
    public readonly static bool purchasedLauncher = false;
    public readonly static bool sawTutorial = false;
    public readonly static bool pauseAvailable = true;
    public readonly static bool playerDead = false;
    public readonly static bool playerWon = false;
}
