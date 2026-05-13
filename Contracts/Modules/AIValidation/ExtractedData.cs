namespace RentGuard.Contracts.Modules.AIValidation;
public record ExtractedData(
    decimal Amount,
    DateTime Date,
    string Reference,
    double Confidence
);
