namespace RentGuard.Presentation.Contracts.Modules.Leases;

public record CreateLeaseRequest(Guid PropertyId, string TenantId, DateTime StartDate, int DueDayOfMonth, decimal MonthlyRent);
