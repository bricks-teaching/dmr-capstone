using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

/// <summary>
/// Represents a single loan.
/// </summary>
public class Loan
{
    public int ID { get; }
    public float total { get; set; }
    public float rate { get; }
    public bool paid { get; set; }
    public LoanType type { get; }
    public int age { get; set; }
    public float originalAmount { get; }
    public string lender { get; }
    public float collateral { get; }
    public bool inGracePeriod { get; set; }

    public Loan(int num, float tot, float rat, LoanType t, float securityDeposit, string creditorId) {
        ID = num;
        total = tot;
        rate = rat;
        paid = false;
        type = t;
        age = 0;
        inGracePeriod = t != LoanType.Payday;
        originalAmount = tot;
        collateral = securityDeposit;
        lender = creditorId;
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

public class CreditorData {
    public bool paid = false;
    public int threatLevel = 0;
    public CreditorData(bool beenPaid, int baseThreat) {
        paid = beenPaid;
        threatLevel = baseThreat;
    }
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
    public static List<Loan> loanList = new List<Loan>();
    //Total debt of the player updated whenever a loan is drawn out, paid or interest is applied
    //used to calculate average income
    public static List<float> income = new List<float>();

    /// <summary>
    /// List of creditor IDs, so state manager can handle processing of
    /// due amounts. The boolean value is whether they've been paid.
    /// </summary>
    public static Dictionary<string, CreditorData> lenders = new Dictionary<string, CreditorData>();
    public static float totalDebt => loanList.Sum(l => l.total);
    public static int nextID { get; set; } = DefaultState.nextID;
    public static int totalLoans => loanList.Count;
    public static int timesEntered { get; set; } = DefaultState.timesEntered;
    public static int totalFloorsVisited { get; set; } = DefaultState.totalFloorsVisited;
    public static int currentFloor { get; set; } = DefaultState.currentFloor;
    public static int scoreChangeFactor { get; set; } = DefaultState.scoreChangeFactor;
    public static float creditScore { get; set; } = DefaultState.creditScore;
    public static int paymentStreak { get; set; } = DefaultState.paymentStreak;
    public static float cashOnEntrance { get; set; } = DefaultState.cashOnEntrance;
    public static float averageIncome => income.Average();
    public static float rateAdjuster = DefaultState.rateAdjuster;
    public static float maxLoanAdjuster = DefaultState.maxLoanAdjuster;
    public static DungeonTypeEnum selectedDungeon { get; set; }
    public static bool sawEntryTutorial = DefaultState.sawEntryTutorial;
    public static bool inStoryTutorial = DefaultState.inStoryTutorial;

    public static List<GameObject> singletons = new List<GameObject>();
    public static bool pauseAvailable = DefaultState.pauseAvailable;
    public static bool playerDead = DefaultState.playerDead;
    public static bool playerWon = DefaultState.playerWon;
    public static bool startedFromDungeon = DefaultState.startedFromDungeon;
    public static float lastCreditScore = 0;
    public static CreditRating creditBarScript;

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
        // in case cash precision got fudged in the dungeon
        cashOnHand = (float)Math.Round(cashOnHand, 2);
        processDueInvoices();

        income.Add(cashOnHand - cashOnEntrance);
        calcCreditScore();
        Debug.Log($"New debt total: {totalDebt}");
        return true;
    }

    /// <summary>
    /// Determines if the loans have been paid regularly.
    /// There are consequences to falling behind and slight rewards for keeping up
    /// </summary>
    private static void processDueInvoices() {
        // go through all loans and raise the threat level if nothing was paid on them
        // while you're at it, apply interest
        Debug.Log($"Processing {loanList.Count} loans");
        foreach (Loan l in loanList) {
            CreditorData cd = lenders[l.lender];
            if (!l.paid) {
                cd.paid = false;
                cd.threatLevel++;
                paymentStreak = 0;
            }
            l.age++;
            l.paid = false;
            if (!l.inGracePeriod) {
                l.total += (float)Math.Round(l.rate * l.total, 2);
            }
            l.inGracePeriod = false;
        }

        // update creditor threat levels if their loans were paid
        foreach (KeyValuePair<string, CreditorData> entry in lenders) {
            CreditorData cd = entry.Value;
            if (cd.paid) {
                paymentStreak++;
                cd.threatLevel--;
            }
        }
    }

    /// <summary>
    /// Calculates the player's credit score.
    /// Used to apply bonuses/penalties to interest rates and maximum loans
    /// </summary>
    public static void calcCreditScore() {
        lastCreditScore = creditScore;
        int sharkPen = 0;
        int oldestLoan = 0;
        foreach (Loan item in loanList) {
            if (item.age > oldestLoan) {
                oldestLoan = item.age;
            }
            if (item.type == LoanType.Payday) {
                sharkPen++;
            }
        }
        if (oldestLoan > 10) {
            creditScore -= scoreChangeFactor * (oldestLoan - 10);
        }
        creditScore -= sharkPen * scoreChangeFactor;
        if (totalLoans > 5) {
            creditScore -= scoreChangeFactor * 2;
        }
        if (totalDebt > 10000) {
            creditScore -= scoreChangeFactor * 8;
        } else if (totalDebt < 5000) {
            creditScore += scoreChangeFactor * 8;
        }
        creditScore += paymentStreak * scoreChangeFactor;
        if (averageIncome <= 0) {
            creditScore -= scoreChangeFactor * 10;
        } else if (averageIncome > totalDebt * 0.03) {
            creditScore += scoreChangeFactor * 15;
        } else {
            creditScore += scoreChangeFactor * 5;
        }

        // Excellent -------------
        if (creditScore > ExcellentCredit.max) {
            creditScore = ExcellentCredit.max;
        }
        if (creditScore >= ExcellentCredit.min && creditScore <= ExcellentCredit.max) {
            currentRating = ExcellentCredit;
        // Good ------------------
        } else if (creditScore >= GoodCredit.min && creditScore < GoodCredit.max) {
            currentRating = GoodCredit;
        // Fair ------------------
        } else if (creditScore >= FairCredit.min && creditScore < FairCredit.max) {
            currentRating = FairCredit;
        // Poor ------------------
        } else if (creditScore < PoorCredit.max && creditScore >= PoorCredit.min) {
            currentRating = PoorCredit;
        // WTF are you doing? ----
        } else if (creditScore < AbysmalCredit.max) {
            currentRating = AbysmalCredit;
        }
        if (creditScore < AbysmalCredit.min) {
            creditScore = AbysmalCredit.min;
        }

        rateAdjuster = DefaultState.rateAdjuster * currentRating.rateModifier;
        maxLoanAdjuster = DefaultState.maxLoanAdjuster * currentRating.loanModifier;
        creditScore = Mathf.Round(creditScore);
        Debug.Log($"Credit score for day {timesEntered}: {creditScore}, delta: {creditScore-lastCreditScore}");
        if (creditBarScript == null) {
            creditBarScript = GameObject.FindObjectOfType<CreditRating>();
            Debug.Log("it was nul");
        }
        creditBarScript.updateRatingBar();
    }

    public static CreditScore AbysmalCredit = new CreditScore {
        min = 300,
        max = 350,
        rating = "Abysmal",
        rateModifier = 2.0f,
        loanModifier = 0.5f
    };

    public static CreditScore PoorCredit = new CreditScore {
        min = 350,
        max = 450,
        rating = "Poor",
        rateModifier = 1.5f,
        loanModifier = 0.75f
    };

    public static CreditScore FairCredit = new CreditScore {
        min = 450,
        max = 550,
        rating = "Fair",
        rateModifier = 1f,
        loanModifier = 1f
    };

    public static CreditScore GoodCredit = new CreditScore {
        min = 550,
        max = 650,
        rating = "Good",
        rateModifier = 0.9f,
        loanModifier = 1.2f
    };

    public static CreditScore ExcellentCredit = new CreditScore {
        min = 650,
        max = 850,
        rating = "Excellent",
        rateModifier = 0.75f,
        loanModifier = 1.6f
    };

    public static CreditScore currentRating = FairCredit;

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
        totalFloorsVisited = DefaultState.totalFloorsVisited;
        currentFloor = DefaultState.currentFloor;
        scoreChangeFactor = DefaultState.scoreChangeFactor;
        lastCreditScore = 0;
        creditScore = DefaultState.creditScore;
        paymentStreak = DefaultState.paymentStreak;
        cashOnEntrance = DefaultState.cashOnEntrance;
        sawEntryTutorial = DefaultState.sawEntryTutorial;
        inStoryTutorial = DefaultState.inStoryTutorial;
        destroyAllSingletons();
        pauseAvailable = DefaultState.pauseAvailable;
        playerDead = DefaultState.playerDead;
        playerWon = DefaultState.playerWon;
        startedFromDungeon = DefaultState.startedFromDungeon;
        currentRating = FairCredit;
    }

    public static void destroyAllSingletons() {
        foreach (GameObject go in singletons) {
            if (go != null) {
                Debug.Log($"Destroying {go.name} during state reset");
                UnityEngine.Object.Destroy(go);
            }
        }
        singletons.Clear();
        lenders.Clear();
        rateAdjuster = DefaultState.rateAdjuster;
        maxLoanAdjuster = DefaultState.maxLoanAdjuster;
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
    public readonly static int totalFloorsVisited = 0;
    public readonly static int scoreChangeFactor = 3;
    public readonly static int creditScore = 500;
    public readonly static int paymentStreak = 0;
    public readonly static float cashOnEntrance = 0.0f;
    public readonly static float rateAdjuster = 1.0f;
    public readonly static float maxLoanAdjuster = 1.0f;
    public readonly static bool sawEntryTutorial = false;
    public readonly static bool inStoryTutorial = false;
    public readonly static bool pauseAvailable = true;
    public readonly static bool playerDead = false;
    public readonly static bool playerWon = false;
    public readonly static bool startedFromDungeon = true;
}

public struct CreditScore {
    public int min;
    public int max;
    public string rating;

    public float rateModifier;
    public float loanModifier;

    public int range => max - min;
}
