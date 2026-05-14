using Microsoft.EntityFrameworkCore;
using RentGuard.Core.Business.Shared.Outbox;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class EfOutboxRepository : IOutboxRepository
{
    private readonly RentGuardDbContext _context;

    public EfOutboxRepository(RentGuardDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(OutboxMessage message)
    {
        await _context.OutboxMessages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize)
    {
        return await _context.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task UpdateAsync(OutboxMessage message)
    {
        _context.OutboxMessages.Update(message);
        await _context.SaveChangesAsync();
    }
}
