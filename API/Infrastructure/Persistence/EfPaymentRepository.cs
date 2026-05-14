using Microsoft.EntityFrameworkCore;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class EfPaymentRepository : IPaymentRepository
{
    private readonly RentGuardDbContext _context;

    public EfPaymentRepository(RentGuardDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }
}

