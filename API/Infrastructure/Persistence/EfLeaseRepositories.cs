using Microsoft.EntityFrameworkCore;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class EfPropertyRepository : IPropertyRepository
{
    private readonly RentGuardDbContext _context;

    public EfPropertyRepository(RentGuardDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Property property)
    {
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();
    }

    public async Task<Property?> GetByIdAsync(Guid id)
    {
        return await _context.Properties.FindAsync(id);
    }

    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        return await _context.Properties.ToListAsync();
    }
}

public class EfLeaseRepository : ILeaseRepository
{
    private readonly RentGuardDbContext _context;

    public EfLeaseRepository(RentGuardDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Lease lease)
    {
        await _context.Leases.AddAsync(lease);
        await _context.SaveChangesAsync();
    }

    public async Task<Lease?> GetByIdAsync(Guid id)
    {
        return await _context.Leases.FindAsync(id);
    }

    public async Task<Lease?> GetByIdForUpdateAsync(Guid id)
    {
        // Native SQL for UPDLOCK to guarantee atomic settlement
        return await _context.Leases
            .FromSqlRaw("SELECT * FROM Leases WITH (UPDLOCK, READPAST) WHERE Id = {0}", id)
            .FirstOrDefaultAsync();
    }

    public async Task<Lease?> GetByResidentIdAsync(string residentId)
    {
        return await _context.Leases
            .FirstOrDefaultAsync(l => l.ResidentId == residentId && l.IsActive);
    }

    public async Task UpdateAsync(Lease lease)
    {
        _context.Leases.Update(lease);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Lease>> GetAllAsync()
    {
        return await _context.Leases.ToListAsync();
    }
}

