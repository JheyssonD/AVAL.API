using Microsoft.EntityFrameworkCore;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class EfTrustScoreRepository : ITrustScoreRepository
{
    private readonly RentGuardDbContext _context;

    public EfTrustScoreRepository(RentGuardDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetCurrentScoreAsync(string residentId)
    {
        var lastScore = await _context.TrustScoreHistory
            .Where(h => h.ResidentId == residentId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => (int?)h.NewScore)
            .FirstOrDefaultAsync();

        return lastScore ?? 80;
    }

    public async Task UpdateScoreAsync(string residentId, int newScore)
    {
        await Task.CompletedTask;
    }

    public async Task AddHistoryAsync(TrustScoreHistory history)
    {
        await _context.TrustScoreHistory.AddAsync(history);
        await _context.SaveChangesAsync();
    }

    public async Task AddSnapshotAsync(TrustScoreSnapshot snapshot)
    {
        await _context.TrustScoreSnapshots.AddAsync(snapshot);
        await _context.SaveChangesAsync();
    }

    public async Task RunBatchTrajectoryCalculationAsync(Guid tenantId)
    {
        // El script real se ejecutaría vía ExecuteSqlRawAsync si es muy complejo
        const string sql = "-- TODO: Invocar script de regresión lineal por lotes";
        await _context.Database.ExecuteSqlRawAsync(sql, new { TenantId = tenantId });
    }
}
