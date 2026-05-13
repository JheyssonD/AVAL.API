using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;

namespace RentGuard.Core.Business.Modules.Leases.CreateLease;

public record CreateLeaseCommand(Guid PropertyId, string TenantId, DateTime StartDate, int DueDayOfMonth);

public class CreateLeaseHandler
{
    private readonly ILeaseRepository _leaseRepository;
    private readonly IPropertyRepository _propertyRepository;

    public CreateLeaseHandler(ILeaseRepository leaseRepository, IPropertyRepository propertyRepository)
    {
        _leaseRepository = leaseRepository;
        _propertyRepository = propertyRepository;
    }

    public async Task Handle(CreateLeaseCommand command, CancellationToken ct)
    {
        // BLINDAJE: Validar existencia de la propiedad
        var property = await _propertyRepository.GetByIdAsync(command.PropertyId);
        if (property == null) throw new ArgumentException("Property not found.");

        var lease = Lease.Create(command.PropertyId, command.TenantId, command.StartDate, command.DueDayOfMonth);
        await _leaseRepository.AddAsync(lease);
    }
}
