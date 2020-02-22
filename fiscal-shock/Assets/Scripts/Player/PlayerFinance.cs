public static class PlayerFinance
{
    public static float cashOnHand { get; set; } = 1000.0f;
    public static float debtBank { get; set; } = 2500.0f;
    public static float bankMaxLoan { get; set; } = 10000.0f;
    public static float bankInterestRate { get; set; } = 0.035f;
    public static int bankThreatLevel { get; set; }
    public static float debtMob { get; set; }
    public static float mobMaxLoan { get; set; } = 4000.0f;
    public static float mobInterestRate { get; set; } = 0.155f;
    public static int mobThreatLevel { get; set; } = 3;
}
