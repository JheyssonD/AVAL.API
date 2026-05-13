namespace RentGuard.Contracts.Modules.Review;

public enum ReviewDecision
{
    Approved,
    Rejected
}

public record SubmitReviewRequest(
    Guid PaymentId,
    ReviewDecision Decision,
    string ReviewerId,
    string Comments
);
