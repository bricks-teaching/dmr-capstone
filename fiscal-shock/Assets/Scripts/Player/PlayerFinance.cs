public static class PlayerFinance {
    public static float cashOnHand { get; set; } = 1000.0f;
    public static float totalDebt {get; set;}
    public static float totalDebtBank {get; set; } = 2500.0f;
    public static float totalDebtShark { get; set; } = 0.0f;

    public static bool startNewDay() {
        if (totalDebtShark > 0) {
            //If unpaid debts present up threat level
            SharkScript.sharkUnpaid();
            //activates interest method in sharkscript
            SharkScript.sharkInterest();
            SharkScript.sharkDue = true;
        }
        if (totalDebtBank > 0) {
            //If unpaid debts present up threat level
            ATMScript.bankUnpaid();
            //activates interest method in atmscript
            ATMScript.bankInterest();
            ATMScript.bankDue = true;
        }
        return true;
    }
}
