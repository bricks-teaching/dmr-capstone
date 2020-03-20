using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loan
{
    public int ID;// {get; set;}
    public float total;// {get; set;}
    public float rate;// {get; set;}
    public bool paid;// {get; set;}
    public bool source;// {get; set;}
    public int age;// {get; set;}

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

public class StateManager : MonoBehaviour
{
    //list of loans that the player posesses
    public static LinkedList<Loan> loanList = new LinkedList<Loan>();
    //Total debt of the player updated whenever a loan is drawn out, paid or interest is applied
    public static float totalDebt {get; set;}
    public static int nextID {get; set;} = 0;
    public static int totalLoans {get; set;}
    public static int timesEntered {get; set;} = 0;
    public static int currentFloor {get; set;} = 0;
    public static int creditScore {get; set;}
    public static int paymentStreak {get; set;}

    public static void calcCreditScore()
    {
        int baseScore = 500, sharkPen = 0;
        int oldestLoan = 0;
        foreach (Loan item in loanList)
        {
            if(item.age++ > oldestLoan){
                oldestLoan = item.age;
            }
            if(item.source){
                sharkPen++;
            }
        }
        if(oldestLoan > 10){
            baseScore -= 7 * (oldestLoan - 18);
        }
        baseScore -= sharkPen * 5;
        if(totalLoans > 5){
            baseScore -= 10;
        }
        if(totalDebt > 10000){
            baseScore -= 40;
        } else if (totalDebt > 5000){
            baseScore += 40;
        }
        baseScore += (paymentStreak * 5);
    }

    //calculates debt total
    public static void calcDebtTotals()
    {
        totalDebt = SharkScript.sharkTotal + ATMScript.bankTotal;
    }
}
