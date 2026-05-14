using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class InMemoryLeaseRepository : ILeaseRepository
{
    private readonly List<Lease> _leases = new();
    
    public Task AddAsync(Lease lease) 
    { 
        _leases.Add(lease); 
        return Task.CompletedTask; 
    }

    public Task<Lease?> GetByIdAsync(Guid id) => Task.FromResult(_leases.FirstOrDefault(x => x.Id == id));

    public Task<Lease?> GetByIdForUpdateAsync(Guid id) => GetByIdAsync(id); // In-memory doesn't need real UPDLOCK for unit tests

    public Task UpdateAsync(Lease lease)
    {
        var existing = _leases.FirstOrDefault(x => x.Id == lease.Id);
        if (existing != null)
        {
            _leases.Remove(existing);
            _leases.Add(lease);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Lease>> GetAllAsync() => Task.FromResult(_leases.AsEnumerable());
}
