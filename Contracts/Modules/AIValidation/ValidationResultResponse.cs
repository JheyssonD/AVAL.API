namespace RentGuard.Contracts.Modules.AIValidation;
public record ValidationResultResponse(
    bool RequiresManualReview,
    string Reason,
    ExtractedData Data
);
