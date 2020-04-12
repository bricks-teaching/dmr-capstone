using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

[System.Serializable]
public class ValidLoan : MonoBehaviour
{
    [Tooltip("Lender ID")]
    public String creditorID;
    [Tooltip("Type of loan")]
    public LoanType loanType;

    [Tooltip("Interest rate for this loan type")]
    [Range(0, 1)]
    public float interestRate;

    [Tooltip("Percentage of the loan amount added as collateral")]
    [Range(0, 1)]
    public float collateralAmountPercent = 0;  // added to something

    [Tooltip("Multiplier on the interest rate for loans with collateral")]
    [Range(0, 1)]
    public float collateralRateReduction = 1;  // it's a multiplier on interestRate so 1 = no effect
    [Tooltip("Dialog on success when adding a loan")]
    public String successText;
    [Tooltip("Dialog on failure when adding a loan")]
    public String failureText;
    [Tooltip("Dialog on error when adding a loan")]
    public String errorText;
    [Tooltip("Dialog on success when paying")]
    public String successPaidText;
    [Tooltip("Dialog on failure when paying")]
    public String failurePaidText;
    [Tooltip("Dialog on error when paying")]
    public String errorPaidText;
    [Tooltip("Input for adding loan of this type")]
    public TMP_InputField addLoanInput;
}
