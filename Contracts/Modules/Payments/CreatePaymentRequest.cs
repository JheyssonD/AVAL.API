namespace RentGuard.Contracts.Modules.Payments;

public record CreatePaymentRequest(
    decimal Amount,
    string Currency,
    string Reference,
    string TenantId,
    string PropertyId,
    DateTime PaymentDate
);
