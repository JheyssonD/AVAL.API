namespace RentGuard.Contracts.Modules.Identity;

public record UserResponse(
    Guid Id,
    string Email,
    UserRole Role,
    string TenantId
);
