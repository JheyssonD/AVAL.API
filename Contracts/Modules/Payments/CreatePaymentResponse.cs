namespace RentGuard.Contracts.Modules.Payments;

public record CreatePaymentResponse(
    Guid Id,
    string Status,
    DateTime CreatedAt
);
