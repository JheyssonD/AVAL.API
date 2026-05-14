using Dapper;
using Microsoft.Data.SqlClient;
using RentGuard.Core.Business.Shared;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class SqlTrustScoreRepository : ITrustScoreRepository
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;

    public SqlTrustScoreRepository(IConfiguration configuration, ITenantContext tenantContext)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _tenantContext = tenantContext;
    }

    public async Task<int> GetCurrentScoreAsync(string residentId)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT TOP 1 NewScore FROM TrustScoreHistory WHERE ResidentId = @ResidentId AND TenantId = @TenantId ORDER BY CreatedAt DESC";
        var lastScore = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { ResidentId = residentId, TenantId = _tenantContext.TenantId });
        return lastScore ?? 80; // Puntaje inicial por defecto
    }

    public async Task UpdateScoreAsync(string residentId, int newScore)
    {
        // El score se actualiza de forma efectiva mediante la insercin en el historial (Audit-Only)
        await Task.CompletedTask; 
    }

    public async Task AddHistoryAsync(TrustScoreHistory history)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO TrustScoreHistory (Id, TenantId, ResidentId, PreviousScore, NewScore, Reason, CreatedAt)
            VALUES (@Id, @TenantId, @ResidentId, @PreviousScore, @NewScore, @Reason, @CreatedAt)";
        
        await connection.ExecuteAsync(sql, new {
            history.Id,
            TenantId = _tenantContext.TenantId,
            history.ResidentId,
            history.PreviousScore,
            history.NewScore,
            history.Reason,
            history.CreatedAt
        });
    }

    public async Task AddSnapshotAsync(TrustScoreSnapshot snapshot)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO TrustScoreSnapshots (Id, TenantId, UserId, ScoreDate, ScoreValue, Status)
            VALUES (@Id, @TenantId, @UserId, @ScoreDate, @ScoreValue, @Status)";
        
        await connection.ExecuteAsync(sql, new {
            snapshot.Id,
            TenantId = _tenantContext.TenantId,
            snapshot.UserId,
            snapshot.ScoreDate,
            snapshot.ScoreValue,
            snapshot.Status
        });
    }

    public async Task RunBatchTrajectoryCalculationAsync(Guid tenantId)
    {
        // Forzar que el tenantId pasado coincida con el contexto si es necesario, 
        // o usar el del contexto directamente.
        var targetTenantId = _tenantContext.TenantId ?? tenantId;

        using var connection = new SqlConnection(_connectionString);
        
        // El script real estar en d:\Repositories\AVAL\Presentation\API\Infrastructure\Persistence\Scripts\UpdateTrustInsights.sql
        const string sql = "-- TODO: Invocar script de regresin lineal por lotes filtrado por @TenantId";
        await connection.ExecuteAsync(sql, new { TenantId = targetTenantId });
    }
}
