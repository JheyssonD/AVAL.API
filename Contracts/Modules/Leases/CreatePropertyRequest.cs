namespace RentGuard.Presentation.Contracts.Modules.Leases;

public record CreatePropertyRequest(string Title, string Address, decimal Rent, string Currency, string LandlordId);
