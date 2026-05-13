namespace RentGuard.Contracts.Modules.TrustScore;

public enum TrustScoreEvent
{
    EarlyPayment,    // +15
    OnTimePayment,   // +10
    LatePayment,     // -20
    PartialPayment,  // 0
    RejectedPayment  // -5
}
