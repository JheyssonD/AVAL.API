using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using System.Collections.Concurrent;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class InMemoryLeaseRepository : IPropertyRepository, ILeaseRepository
{
    private readonly ConcurrentDictionary<Guid, Property> _properties = new();
    private readonly ConcurrentBag<Lease> _leases = new();

    public Task AddAsync(Property property)
    {
        _properties[property.Id] = property;
        return Task.CompletedTask;
    }

    public Task AddAsync(Lease lease)
    {
        _leases.Add(lease);
        return Task.CompletedTask;
    }

    public Task<Property?> GetByIdAsync(Guid id)
    {
        _properties.TryGetValue(id, out var property);
        return Task.FromResult(property);
    }
}
