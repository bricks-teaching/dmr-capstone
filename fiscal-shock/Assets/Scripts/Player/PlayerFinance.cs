﻿public static class PlayerFinance {
    public static float cashOnHand { get; set; } = 1000.0f;
    public static float debtBank {get; set; } = 2500.0f;
    public static float bankMaxLoan { get; set; } = 10000.0f;
    public static float bankInterestRate { get; set; } = 0.035f;
    public static int bankThreatLevel { get; set; } = 0;
    public static float debtShark { get; set; } = 0.0f;
    public static float sharkMaxLoan { get; set; } = 4000.0f;
    public static float sharkInterestRate { get; set; } = 0.155f;
    public static int sharkThreatLevel { get; set; } = 3;

    public static bool startNewDay() {
        if (debtShark > 0) {
            sharkThreatLevel++;
            debtShark += debtShark * sharkInterestRate;
            SharkScript.sharkDue = true;
        }
        if (debtBank > 0) {
            bankThreatLevel++;
            debtBank += debtBank * bankInterestRate;
            ATMScript.bankDue = true;
        }
        return true;
    }
}
